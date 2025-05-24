namespace OrchardCore.Indexing.Models;

public sealed class UpdatedContext<T> : HandlerContextBase<T>
{
    public UpdatedContext(T model)
        : base(model)
    {
    }
}
