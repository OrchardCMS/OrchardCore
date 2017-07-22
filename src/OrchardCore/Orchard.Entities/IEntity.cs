using Newtonsoft.Json.Linq;

namespace Orchard.Entities
{
    public interface IEntity
    {
        JObject Properties { get; }
    }
}
