using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Hosting.CommandConsole.HostContext
{
    public class CommandHostContext {
        public CommandReturnCodes StartSessionResult { get; set; }
        public CommandReturnCodes RetryResult { get; set; }

        public OrchardParameters Arguments { get; set; }
        public DirectoryInfo OrchardDirectory { get; set; }
        public bool DisplayUsageHelp { get; set; }
    }
}
