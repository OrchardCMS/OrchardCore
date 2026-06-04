namespace OrchardCore.DisplayManagement.Implementation;

public interface IShapeFactoryEvents
{
    void Creating(ShapeCreatingContext context);
    void Created(ShapeCreatedContext context);
}

public class ShapeCreatingContext
{
    private IList<Func<ShapeCreatedContext, Task>> _onCreated;
    protected internal Func<ValueTask<IShape>> _createAsync;

    public IServiceProvider ServiceProvider { get; set; }

    public IShapeFactory ShapeFactory { get; set; }

    public dynamic New { get; set; }

    public string ShapeType { get; set; }

    public Func<ValueTask<IShape>> CreateAsync
    {
        get => CreateInternalAsync;
        set => _createAsync = value;
    }

    public IList<Func<ShapeCreatedContext, Task>> OnCreated { get => _onCreated ??= []; }

    internal bool HasOnCreated => _onCreated != null && _onCreated.Count > 0;

    public Func<IShape> Create
    {
        set => CreateAsync = () => ValueTask.FromResult(value());
    }

    internal virtual ValueTask<IShape> CreateInternalAsync()
    {
        return _createAsync != null ? _createAsync() : ValueTask.FromResult<IShape>(null);
    }
}

internal class ShapeCreatingContext<TState> : ShapeCreatingContext
{
    public TState State { get; set; }

    public Func<TState, ValueTask<IShape>> CreateAsyncWithState { get; set; }

    internal override ValueTask<IShape> CreateInternalAsync()
    {
        if (_createAsync == null && CreateAsyncWithState != null)
        {
            return CreateAsyncWithState(State);
        }

        return base.CreateInternalAsync();
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
