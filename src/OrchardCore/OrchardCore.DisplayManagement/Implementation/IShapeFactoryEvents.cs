using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Implementation
{
    public interface IShapeFactoryEvents
    {
        void Creating(ShapeCreatingContext context);
        void Created(ShapeCreatedContext context);
    }

    public class ShapeCreatingContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IShapeFactory ShapeFactory { get; set; }
        public dynamic New { get; set; }
        public string ShapeType { get; set; }
        public Func<ValueTask<IShape>> CreateAsync { get; set; }
        public IList<Func<ShapeCreatedContext, Task>> OnCreated { get; set; }

        public Func<IShape> Create
        {
            set => CreateAsync = () => new ValueTask<IShape>(value());
        }
    }

    public class ShapeCreatedContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IShapeFactory ShapeFactory { get; set; }
        public dynamic New { get; set; }
        public string ShapeType { get; set; }
        public IShape Shape { get; set; }
    }

    public abstract class ShapeFactoryEvents : IShapeFactoryEvents
    {
        public virtual void Creating(ShapeCreatingContext context)
        {
        }
        public virtual void Created(ShapeCreatedContext context)
        {
        }
    }
}
