namespace OrchardCore.Indexing.Models;

public class ModelEntry<T>
{
    public T Model { get; set; }

    public dynamic Shape { get; set; }
}
