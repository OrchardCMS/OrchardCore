namespace OrchardCore.Indexing.Models;

public sealed class ValidatingContext<T> : HandlerContextBase<T>
{
    public ValidationResultDetails Result { get; } = new();

    public ValidatingContext(T model)
        : base(model)
    {
    }
}
