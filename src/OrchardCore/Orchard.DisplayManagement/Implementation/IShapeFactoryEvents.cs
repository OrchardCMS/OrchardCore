using System;
using System.Collections.Generic;

namespace Orchard.DisplayManagement.Implementation
{
    public interface IShapeFactoryEvents
    {
        void Creating(ShapeCreatingContext context);
        void Created(ShapeCreatedContext context);
    }

    public class ShapeCreatingContext
    {
        public IShapeFactory ShapeFactory { get; set; }
        public dynamic New { get; set; }
        public string ShapeType { get; set; }
        public Func<dynamic> Create { get; set; }
        public IList<Action<ShapeCreatedContext>> OnCreated { get; set; }
    }

    public class ShapeCreatedContext
    {
        public IShapeFactory ShapeFactory { get; set; }
        public dynamic New { get; set; }
        public string ShapeType { get; set; }
        public dynamic Shape { get; set; }
    }

    public abstract class ShapeFactoryEvents : IShapeFactoryEvents
    {
        public virtual void Creating(ShapeCreatingContext context) { }
        public virtual void Created(ShapeCreatedContext context) { }
    }
}