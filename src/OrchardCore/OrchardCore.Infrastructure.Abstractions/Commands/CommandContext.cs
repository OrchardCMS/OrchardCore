using System.Collections.Generic;
using System.IO;

namespace OrchardCore.Environment.Commands
{
    public class CommandContext
    {
        public TextReader Input { get; set; }
        public TextWriter Output { get; set; }

        public string Command { get; set; }
        public IEnumerable<string> Arguments { get; set; }
        public IDictionary<string, string> Switches { get; set; }

        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
