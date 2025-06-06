namespace OrchardCore.Infrastructure.Entities;

public sealed class UpdatedContext<T> : HandlerContextBase<T>
{
    public UpdatedContext(T model)
        : base(model)
    {
    }
}
