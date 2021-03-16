using System.Collections.Generic;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Features.Models
{
    /// <summary>
    /// Represents a module.
    /// </summary>
    public class ModuleEntry
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModuleEntry()
        {
            Notifications = new List<string>();
        }

        /// <summary>
        /// The module's extension descriptor.
        /// </summary>
        public IExtensionInfo Descriptor { get; set; }

        /// <summary>
        /// Boolean value indicating if the module needs a version update.
        /// </summary>
        public bool NeedsVersionUpdate { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature was recently installed.
        /// </summary>
        public bool IsRecentlyInstalled { get; set; }

        /// <summary>
        /// List of module notifications.
        /// </summary>
        public List<string> Notifications { get; set; }

        /// <summary>
        /// Indicates whether the module can be uninstalled by the user.
        /// </summary>
        public bool CanUninstall { get; set; }
    }
}
