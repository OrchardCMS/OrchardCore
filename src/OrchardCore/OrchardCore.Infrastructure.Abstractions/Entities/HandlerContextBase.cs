namespace OrchardCore.Infrastructure.Entities;

public abstract class HandlerContextBase<T>
{
    public T Model { get; }

    public HandlerContextBase(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Model = model;
    }
}
