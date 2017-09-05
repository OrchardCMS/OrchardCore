using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities
{
    public interface IEntity
    {
        JObject Properties { get; }
    }
}
