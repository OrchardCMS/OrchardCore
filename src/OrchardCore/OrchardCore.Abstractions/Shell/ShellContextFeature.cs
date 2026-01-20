using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Used to capture the shell context and original path infos.
    /// </summary>
    public class ShellContextFeature
    {
        /// <summary>
        /// The current shell context.
        /// </summary>
        public ShellContext ShellContext { get; set; }

        /// <summary>
        /// The original path base.
        /// </summary>
        public PathString OriginalPathBase { get; set; }

        /// <summary>
        /// The original path.
        /// </summary>
        public PathString OriginalPath { get; set; }
    }
}
