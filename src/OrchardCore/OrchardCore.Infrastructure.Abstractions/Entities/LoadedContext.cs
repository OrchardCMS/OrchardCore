namespace OrchardCore.Infrastructure.Entities;

public sealed class LoadedContext<T> : HandlerContextBase<T>
{
    public LoadedContext(T model)
        : base(model)
    {
    }
}
