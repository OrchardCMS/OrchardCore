using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Hosting.CommandConsole.Parameters {
    public class CommandParameters {
        public IList<string> Arguments { get; set; }
        public IDictionary<string, string> Switches { get; set; }
    }
}
