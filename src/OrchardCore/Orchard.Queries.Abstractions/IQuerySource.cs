using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Orchard.Queries
{
    public interface IQuerySource
    {
        string Name { get; }
        Task<JToken> ExecuteQueryAsync(Query query);
    }
}
