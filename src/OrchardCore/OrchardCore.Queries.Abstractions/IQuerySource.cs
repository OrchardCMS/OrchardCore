using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Queries
{
    public interface IQuerySource
    {
        string Name { get; }
        Query Create();
        Task<object> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
    }
}
