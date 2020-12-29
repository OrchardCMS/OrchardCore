using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Used to capture the shell settings and original path infos.
    /// </summary>
    public class ShellSettingsFeature
    {
        /// <summary>
        /// The current shell settings.
        /// </summary>
        public ShellSettings ShellSettings { get; set; }

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
