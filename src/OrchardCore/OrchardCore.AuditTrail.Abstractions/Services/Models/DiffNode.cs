using Newtonsoft.Json.Linq;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DiffNode
    {
        public DiffType Type { get; set; }
        public string Context { get; set; }
        public JToken Previous { get; set; }
        public JToken Current { get; set; }
    }

    public enum DiffType
    {
        Change
    }
}
