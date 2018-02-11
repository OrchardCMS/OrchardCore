using System;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.DisplayManagement.Manifest
{
    /// <summary>
    /// Defines a Theme which is a dedicated Module for theming purposes.
    /// If the Theme has only one default feature, it may be defined there.
    /// </summary>

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ThemeAttribute : ModuleAttribute
    {
        /// <param name="Name">
        /// Human-readable name of the theme. If not provided, the assembly name will be used.
        /// </param>
        /// <param name="Author">The developer of the theme.</param>
        /// <param name="Website">The URL for the website of the theme developer.</param>
        /// <param name="Version">The version number of the theme in SemVer format.</param>
        /// <param name="Tags">A comma-separated lists of tags for the theme.</param>
        /// <param name="BaseTheme">The base theme if the theme is derived from an existing one.</param>
        /// <param name="Description">A brief summary of what the default feature does.</param>
        /// <param name="Dependencies">
        /// A comma-separated list of features that the default feature depends on.
        /// So that its drivers / handlers are invoked after those of dependencies.
        /// </param>
        /// <param name="Priority">
        /// The priority of the default feature without breaking the dependencies order.
        /// higher is the priority, later the drivers / handlers are invoked.
        /// </param>
        /// <param name="Category">
        /// The group (by category) that the default feature belongs.
        /// If not provided, defaults to 'Uncategorized'.</param>

        public ThemeAttribute(
            string Name = null,
            string Author = "",
            string Website = "",
            string Version = "0.0",
            string Tags = "",
            string BaseTheme = null,
            string Description = "",
            string Dependencies = "",
            string Priority = "0",
            string Category = null)
            : base(Name, Author, Website, Version, Tags, Description, Dependencies, Priority, Category)
        {
            baseTheme = BaseTheme;
        }

        public override string Type => "Theme";
        public string baseTheme { get; }
    }
}