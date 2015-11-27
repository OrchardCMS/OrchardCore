using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using System.Reflection;

namespace Orchard.DisplayManagement.Implementation
{
    public class DefaultShapeFactory : Composite, IShapeFactory
    {
        private readonly IEnumerable<IShapeFactoryEvents> _events;
        private readonly IShapeTableManager _shapeTableManager;

        public DefaultShapeFactory(
            IEnumerable<IShapeFactoryEvents> events,
            IShapeTableManager shapeTableManager)
        {
            _events = events;
            _shapeTableManager = shapeTableManager;
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Create(binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames));
            return true;
        }

        public IShape Create(string shapeType)
        {
            return Create(shapeType, Arguments.Empty(), () => new Shape());
        }

        public IShape Create(string shapeType, INamedEnumerable<object> parameters)
        {
            return Create(shapeType, parameters, () => new Shape());
        }

        public IShape Create(string shapeType, INamedEnumerable<object> parameters, Func<dynamic> createShape)
        {
            var defaultShapeTable = _shapeTableManager.GetShapeTable(null);
            ShapeDescriptor shapeDescriptor;
            defaultShapeTable.Descriptors.TryGetValue(shapeType, out shapeDescriptor);

            parameters = parameters ?? Arguments.Empty();

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
                ShapeType = creatingContext.ShapeType,
                Shape = creatingContext.Create()
            };

            if (!(createdContext.Shape is IShape))
            {
                throw new InvalidOperationException("Invalid base type for shape: " + createdContext.Shape.GetType().ToString());
            }

            ShapeMetadata shapeMetadata = createdContext.Shape.Metadata;
            createdContext.Shape.Metadata.Type = shapeType;

            if (shapeDescriptor != null)
                shapeMetadata.Wrappers = shapeMetadata.Wrappers.Concat(shapeDescriptor.Wrappers).ToList();

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


            // other properties passed with call overlay any defaults, so are after the created events

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

            return createdContext.Shape;
        }
    }
}