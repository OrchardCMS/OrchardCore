using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Theming;

namespace Orchard.DisplayManagement.Implementation
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
            result = ShapeFactoryExtensions.Create(this, binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames));
			return true;
        }

        private ShapeTable GetShapeTable()
        {
            if (_scopedShapeTable == null)
            {
                var theme = _themeManager.GetThemeAsync().GetAwaiter().GetResult();
                _scopedShapeTable = _shapeTableManager.GetShapeTable(theme?.Id);
            }

            return _scopedShapeTable;
        }

        public IShape Create(string shapeType, Func<dynamic> shapeFactory)
        {
            ShapeDescriptor shapeDescriptor;
            GetShapeTable().Descriptors.TryGetValue(shapeType, out shapeDescriptor);

            var creatingContext = new ShapeCreatingContext
            {
                New = this,
                ShapeFactory = this,
                ShapeType = shapeType,
                OnCreated = new List<Action<ShapeCreatedContext>>()
            };
            
            creatingContext.Create = shapeFactory;

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

            // create the new instance
            var createdContext = new ShapeCreatedContext
            {
                New = creatingContext.New,
                ShapeFactory = creatingContext.ShapeFactory,
                ShapeType = creatingContext.ShapeType,
                Shape = creatingContext.Create()
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

            return createdContext.Shape;
        }
    }

}