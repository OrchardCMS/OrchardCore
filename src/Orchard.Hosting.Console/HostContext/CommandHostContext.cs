using Orchard.Hosting.Console.Host;
using System.IO;

namespace Orchard.Hosting.Console.HostContext {
    public class CommandHostContext
    {
        public CommandReturnCodes StartSessionResult { get; set; }
        public CommandReturnCodes RetryResult { get; set; }

        public OrchardParameters Arguments { get; set; }
        public DirectoryInfo OrchardDirectory { get; set; }
        public bool DisplayUsageHelp { get; set; }
    }
}
