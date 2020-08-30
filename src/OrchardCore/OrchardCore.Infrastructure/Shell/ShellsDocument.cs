using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Environment.Shell
{
    public class ShellsDocument : Document
    {
        public Dictionary<string, Shell> Shells { get; set; } = new Dictionary<string, Shell>();
    }

    public class Shell
    {
        public string Name { get; set; }
        public string ReleaseId { get; set; }
        public string ReloadId { get; set; }
    }
}
