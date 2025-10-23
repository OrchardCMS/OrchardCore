namespace OrchardCore.Catalogs.Models;

public class ListSourceModelEntryViewModel<T, TName> : ListSourceModelViewModel<TName>
{
    public IList<CatalogEntryViewModel<T>> Models { get; set; }
}
