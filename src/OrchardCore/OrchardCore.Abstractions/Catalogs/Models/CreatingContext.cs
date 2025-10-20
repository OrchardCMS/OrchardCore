namespace OrchardCore.Catalogs.Models;

public sealed class CreatingContext<T> : HandlerContextBase<T>
{
    public CreatingContext(T model)
        : base(model)
    {
    }
}
