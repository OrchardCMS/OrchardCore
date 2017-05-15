using Newtonsoft.Json.Linq;

namespace Orchard.Entities
{
    public class Entity : IEntity
    {
        public JObject Properties { get; set; } = new JObject();
    }
}
