using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.ViewModels;

public class ListSourceModelEntryViewModel<T> : ListSourceModelViewModel
{
    public IList<ModelEntry<T>> Models { get; set; }
}

public class ListSourceModelEntryViewModel<T, TName> : ListSourceModelViewModel<TName>
{
    public IList<ModelEntry<T>> Models { get; set; }
}
