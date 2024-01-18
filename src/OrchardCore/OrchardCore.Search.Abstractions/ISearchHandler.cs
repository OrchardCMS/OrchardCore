using System.Threading.Tasks;

namespace OrchardCore.Search.Abstractions;

public interface ISearchHandler
{
    Task SearchedAsync(SearchContext context);
}
