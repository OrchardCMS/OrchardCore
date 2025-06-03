namespace OrchardCore.Infrastructure.Entities;

public sealed class DeletingContext<T> : HandlerContextBase<T>
{
    public DeletingContext(T model)
        : base(model)
    {
    }
}
