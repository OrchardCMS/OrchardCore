using Newtonsoft.Json.Linq;

namespace Orchard.Queries
{
    public interface IQuerySource
    {
        string Name { get; }
        JToken ExecuteQuery(IQuery query);
    }
}
