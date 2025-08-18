namespace OrchardCore.Infrastructure.Entities;

public sealed class CreatingContext<T> : HandlerContextBase<T>
{
    public CreatingContext(T model)
        : base(model)
    {
    }
}
