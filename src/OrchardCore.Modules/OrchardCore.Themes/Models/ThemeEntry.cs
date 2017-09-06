using System.Collections.Generic;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Themes.Models
{
    /// <summary>
    /// Represents a theme.
    /// </summary>
    public class ThemeEntry
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThemeEntry()
        {
            Notifications = new List<string>();
        }

        /// <summary>
        /// Instantiates a theme based on an extension info.
        /// </summary>
        /// <param name="extensionInfo">The extension info.</param>
        public ThemeEntry(IExtensionInfo extensionInfo)
        {
            Extension = extensionInfo;
        }

        /// <summary>
        /// The theme's extension info.
        /// </summary>
        public IExtensionInfo Extension { get; set; }

        /// <summary>
        /// Boolean value indicating whether the theme is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Boolean value indicating whether the theme needs a data update / migration.
        /// </summary>
        public bool NeedsUpdate { get; set; }

        /// <summary>
        /// Boolean value indicating if the module needs a version update.
        /// </summary>
        public bool NeedsVersionUpdate { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature was recently installed.
        /// </summary>
        public bool IsRecentlyInstalled { get; set; }

        /// <summary>
        /// Boolean value indicating if the theme can be uninstalled.
        /// </summary>
        public bool CanUninstall { get; set; }

        /// <summary>
        /// List of theme notifications.
        /// </summary>
        public List<string> Notifications { get; set; }

        /// <summary>
        /// The theme's name.
        /// </summary>
        public string Name { get { return Extension.Manifest.Name; } }

        /// <summary>
        /// Boolean value indicating whether this is an admin theme.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Boolean value indicating whether this is the current theme.
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}
