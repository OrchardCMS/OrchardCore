using System.Collections.Generic;

namespace Orchard.Hosting
{
    public class OrchardParameters
    {
        public bool Verbose { get; set; }
        public string Tenant { get; set; }
        public IList<string> Arguments { get; set; }
        public IList<string> ResponseFiles { get; set; }
        public IDictionary<string, string> Switches { get; set; }
    }
}