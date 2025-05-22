using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.ViewModels;

public class ListModelViewModel
{
    public ModelOptions Options { get; set; }

    public dynamic Pager { get; set; }
}

public class ListModelViewModel<T> : ListModelViewModel
{
    public IList<T> Models { get; set; }
}


public class ListSourceModelViewModel : ListModelViewModel
{
    public IEnumerable<string> Sources { get; set; }
}

public class ListSourceModelViewModel<TName> : ListModelViewModel
{
    public IEnumerable<TName> Sources { get; set; }
}

public class ListSourceModelViewModel<T, TName> : ListModelViewModel<TName>
{
    public IEnumerable<T> Sources { get; set; }
}
