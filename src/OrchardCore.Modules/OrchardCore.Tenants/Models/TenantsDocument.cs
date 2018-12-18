using Newtonsoft.Json.Linq;

namespace OrchardCore.Tenants.Models
{
    public class TenantsDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public JObject Settings { get; set; } = new JObject();
    }
}
