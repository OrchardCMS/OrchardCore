namespace OrchardCore.Infrastructure.Entities;

public class ModelEntry<T>
{
    public T Model { get; set; }

    public dynamic Shape { get; set; }
}
