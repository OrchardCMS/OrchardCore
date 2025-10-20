namespace OrchardCore.Catalogs.Models;

public sealed class ValidatingContext<T> : HandlerContextBase<T>
{
    public ValidationResultDetails Result { get; } = new();

    public ValidatingContext(T model)
        : base(model)
    {
    }
}
