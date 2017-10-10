using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DefaultShapeFactory : DynamicObject, IShapeFactory
    {
        private readonly IEnumerable<IShapeFactoryEvents> _events;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IThemeManager _themeManager;
        private ShapeTable _scopedShapeTable; 

        
        public DefaultShapeFactory(
            IEnumerable<IShapeFactoryEvents> events,
            IShapeTableManager shapeTableManager,
            IThemeManager themeManager)
        {
            _events = events;
            _shapeTableManager = shapeTableManager;
            _themeManager = themeManager;
        }

        public dynamic New { get { return this; } }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            // If the member ends with Async then return the task, otherwise await it here.
            // Example:
            // await New.FooAsync()
            // New.Foo()

            if (binder.Name.EndsWith("Async"))
            {
                var binderName = binder.Name.Substring(binder.Name.Length - "Async".Length);
                result = ShapeFactoryExtensions.CreateAsync(this, binderName, Arguments.From(args, binder.CallInfo.ArgumentNames));
            }
            else
            {
                result = ShapeFactoryExtensions.CreateAsync(this, binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames)).GetAwaiter().GetResult();
            }
            
			return true;
        }

        private async Task<ShapeTable> GetShapeTableAsync()
        {
            if (_scopedShapeTable == null)
            {
                var theme = await _themeManager.GetThemeAsync();
                _scopedShapeTable = _shapeTableManager.GetShapeTable(theme?.Id);
            }

            return _scopedShapeTable;
        }

        public async Task<IShape> CreateAsync(string shapeType, Func<Task<IShape>> shapeFactory, Action<ShapeCreatingContext> creating, Action<ShapeCreatedContext> created)
        {
            ShapeDescriptor shapeDescriptor;
            (await GetShapeTableAsync()).Descriptors.TryGetValue(shapeType, out shapeDescriptor);

            var creatingContext = new ShapeCreatingContext
            {
                New = this,
                ShapeFactory = this,
                ShapeType = shapeType,
                OnCreated = new List<Action<ShapeCreatedContext>>(),
                CreateAsync = shapeFactory
            };

            creating?.Invoke(creatingContext);

            // "creating" events may add behaviors and alter base type
            foreach (var ev in _events)
            {
                ev.Creating(creatingContext);
            }

            if (shapeDescriptor != null)
            {
                foreach (var ev in shapeDescriptor.Creating)
                {
                    ev(creatingContext);
                }
            }

            // Create the new instance
            var createdContext = new ShapeCreatedContext
            {
                New = creatingContext.New,
                ShapeFactory = creatingContext.ShapeFactory,
                ShapeType = creatingContext.ShapeType,
                Shape = await creatingContext.CreateAsync()
            };

            var shape = createdContext.Shape as IShape;

            if (shape == null)
            {
                throw new InvalidOperationException("Invalid base type for shape: " + createdContext.Shape.GetType().ToString());
            }

            if (shape.Metadata == null)
            {
                shape.Metadata = new ShapeMetadata();
            }

            ShapeMetadata shapeMetadata = shape.Metadata;
            shape.Metadata.Type = shapeType;

            // Concatenate wrappers if there are any
            if (shapeDescriptor != null && shapeMetadata.Wrappers.Count + shapeDescriptor.Wrappers.Count > 0)
            {
                shapeMetadata.Wrappers = shapeMetadata.Wrappers.Concat(shapeDescriptor.Wrappers).ToList();
            }

            // "created" events provides default values and new object initialization
            foreach (var ev in _events)
            {
                ev.Created(createdContext);
            }

            if (shapeDescriptor != null)
            {
                foreach (var ev in shapeDescriptor.Created)
                {
                    ev(createdContext);
                }
            }

            if (creatingContext.OnCreated != null)
            {
                foreach (var ev in creatingContext.OnCreated)
                {
                    ev(createdContext);
                }
            }

            created?.Invoke(createdContext);

            return createdContext.Shape;
        }
    }

}