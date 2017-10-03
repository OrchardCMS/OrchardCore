using System;

namespace Orchard.Workflows.Models
{
    [Serializable]
    public class UpdatedActivityModel
    {
        public string ClientId { get; set; }
        public string Data { get; set; }
    }
}
