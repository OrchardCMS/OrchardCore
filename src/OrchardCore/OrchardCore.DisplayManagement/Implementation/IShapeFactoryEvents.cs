namespace OrchardCore.DisplayManagement.Implementation;

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
    public Func<ValueTask<IShape>> CreateAsync
    {
        get => Activator != null ? Activator.CreateAsync : null;
        set => Activator = new DelegateShapeActivator(value);
    }
    public IList<Func<ShapeCreatedContext, Task>> OnCreated { get; set; }

    public Func<IShape> Create
    {
        set => CreateAsync = () => ValueTask.FromResult(value());
    }

    internal IShapeActivator Activator { get; set; }

    private readonly struct DelegateShapeActivator : IShapeActivator
    {
        private readonly Func<ValueTask<IShape>> _factory;

        public DelegateShapeActivator(Func<ValueTask<IShape>> factory)
        {
            _factory = factory;
        }

        public ValueTask<IShape> CreateAsync() => _factory();
    }
}

internal interface IShapeActivator
{
    ValueTask<IShape> CreateAsync();
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
