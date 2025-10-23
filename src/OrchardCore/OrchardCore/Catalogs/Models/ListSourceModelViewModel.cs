namespace OrchardCore.Catalogs.Models;

public class ListSourceModelViewModel : ListCatalogEntryViewModel
{
    public IEnumerable<string> Sources { get; set; }
}

public class ListSourceModelViewModel<T, TName> : ListCatalogEntryViewModel<TName>
{
    public IEnumerable<T> Sources { get; set; }
}

public class ListSourceModelViewModel<TName> : ListCatalogEntryViewModel
{
    public IEnumerable<TName> Sources { get; set; }
}
