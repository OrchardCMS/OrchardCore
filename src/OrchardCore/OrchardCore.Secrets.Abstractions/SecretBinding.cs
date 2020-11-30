using Newtonsoft.Json;

namespace OrchardCore.Secrets
{
    public class SecretBinding
    {
        public string Store { get; set; }
        //TODO remove
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
