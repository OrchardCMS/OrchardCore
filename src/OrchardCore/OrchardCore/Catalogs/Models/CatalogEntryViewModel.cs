namespace OrchardCore.Catalogs.Models;

public class CatalogEntryViewModel<T>
{
    public T Model { get; set; }

    public dynamic Shape { get; set; }
}
