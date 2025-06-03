namespace OrchardCore.Infrastructure.Entities;

public sealed class InitializedContext<T> : HandlerContextBase<T>
{
    public InitializedContext(T model)
        : base(model)
    {
    }
}
