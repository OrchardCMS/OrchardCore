using OrchardCore.Filters.Abstractions.Services;
using OrchardCore.Filters.Query.Services;

namespace OrchardCore.Filters.Query
{
    public interface IQueryParser<T> : IFilterParser<QueryFilterResult<T>> where T : class
    {
    }
}
