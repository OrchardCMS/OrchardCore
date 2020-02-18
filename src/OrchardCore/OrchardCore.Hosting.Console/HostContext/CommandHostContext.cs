using System.IO;
using OrchardCore.Environment.Commands;

namespace OrchardCore.Hosting.HostContext
{
    public class CommandHostContext
    {
        public CommandReturnCodes StartSessionResult { get; set; }
        public CommandReturnCodes RetryResult { get; set; }

        public OrchardParameters Arguments { get; set; }
        public DirectoryInfo OrchardDirectory { get; set; }
        public bool DisplayUsageHelp { get; set; }
        public CommandHostAgent CommandHost { get; internal set; }
    }
}
