namespace OrchardCore.Infrastructure.Entities;

public sealed class ValidatedContext<T> : HandlerContextBase<T>
{
    public ValidationResultDetails Result { get; } = new();

    public ValidatedContext(T model, ValidationResultDetails result)
        : base(model)
    {
        Result = result ?? new();
    }
}
