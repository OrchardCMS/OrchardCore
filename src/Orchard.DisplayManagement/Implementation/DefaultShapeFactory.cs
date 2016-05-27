using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Theming;

namespace Orchard.DisplayManagement.Implementation
{
    public class DefaultShapeFactory : Composite, IShapeFactory
    {
        private readonly IEnumerable<IShapeFactoryEvents> _events;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IThemeManager _themeManager;

        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

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

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Create(binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames));
            return true;
        }

        private class ShapeImplementation : IShape, IPositioned
        {
            public ShapeImplementation(string type)
            {
                Metadata.Type = type;
            }

            public ShapeMetadata Metadata { get; } = new ShapeMetadata();

            public string Position
            {
                get
                {
                    return Metadata.Position;
                }

                set
                {
                    Metadata.Position = value;
                }
            }
        }

        public T Create<T>(string shapeType) where T : class
        {
            return (T)Create(typeof(T), shapeType);
        }

        public object Create(Type baseType, string shapeType)
        {
            IShape shape;

            // Don't generate a proxy for shape types
            if (typeof(IShape).IsAssignableFrom(baseType))
            {
                shape = Activator.CreateInstance(baseType) as IShape;
                shape.Metadata.Type = shapeType;
            }
            else
            {
                var options = new ProxyGenerationOptions();
                options.AddMixinInstance(new ShapeImplementation(shapeType));
                shape = ProxyGenerator.CreateClassProxy(baseType, options) as IShape;
            }

            return shape;
        }

        public IShape Create(string shapeType)
        {
            return Create(shapeType, Arguments.Empty, () => new Shape());
        }

        public IShape Create(string shapeType, INamedEnumerable<object> parameters)
        {
            return Create(shapeType, parameters, () => new Shape());
        }

        public T Create<T>(T obj) where T : class
        {
            return (T)Create(typeof(T).Name, Arguments.Empty, () => obj);
        }

        public IShape Create(string shapeType, INamedEnumerable<object> parameters, Func<dynamic> createShape)
        {
            var theme = _themeManager.GetThemeAsync().Result;

            var defaultShapeTable = _shapeTableManager.GetShapeTable(theme?.Id);
            ShapeDescriptor shapeDescriptor;
            defaultShapeTable.Descriptors.TryGetValue(shapeType, out shapeDescriptor);

            parameters = parameters ?? Arguments.Empty;

            var creatingContext = new ShapeCreatingContext
            {
                New = this,
                ShapeFactory = this,
                ShapeType = shapeType,
                OnCreated = new List<Action<ShapeCreatedContext>>()
            };

            IEnumerable<object> positional = parameters.Positional.ToList();
            var baseType = positional.FirstOrDefault() as Type;

            if (baseType == null)
            {
                // default to common base class
                creatingContext.Create = createShape ?? (() => new Shape());
            }
            else
            {
                // consume the first argument
                positional = positional.Skip(1);
                creatingContext.Create = () => Activator.CreateInstance(baseType);
            }

            // "creating" events may add behaviors and alter base type)
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

            if (!(createdContext.Shape is IShape))
            {
                throw new InvalidOperationException("Invalid base type for shape: " + createdContext.Shape.GetType().ToString());
            }

            if (createdContext.Shape.Metadata == null)
            {
                createdContext.Shape.Metadata = new ShapeMetadata();
            }

            ShapeMetadata shapeMetadata = createdContext.Shape.Metadata;
            createdContext.Shape.Metadata.Type = shapeType;

            // Concatenate wrappers if there are any
            if (shapeDescriptor != null &&
                shapeMetadata.Wrappers.Count + shapeDescriptor.Wrappers.Count > 0)
            {
                shapeMetadata.Wrappers = shapeMetadata.Wrappers.Concat(shapeDescriptor.Wrappers).ToList();
            }

            // only one non-Type, non-named argument is allowed
            var initializer = positional.SingleOrDefault();
            if (initializer != null)
            {
                foreach (var prop in initializer.GetType().GetProperties())
                {
                    createdContext.Shape[prop.Name] = prop.GetValue(initializer, null);
                }
            }

            foreach (var kv in parameters.Named)
            {
                createdContext.Shape[kv.Key] = kv.Value;
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

            foreach (var ev in creatingContext.OnCreated)
            {
                ev(createdContext);
            }

            return createdContext.Shape;
        }
    }

}