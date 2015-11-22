using System.Collections.Generic;

namespace Orchard.Hosting.Parameters
{
    public class CommandParameters
    {
        public IList<string> Arguments { get; set; }
        public IDictionary<string, string> Switches { get; set; }
    }
}