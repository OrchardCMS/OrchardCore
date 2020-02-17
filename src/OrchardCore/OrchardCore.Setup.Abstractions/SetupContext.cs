using System.Collections.Generic;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Services
{
    /// <summary>
    /// Represents a class that contains a set up information.
    /// </summary>
    public class SetupContext
    {
        /// <summary>
        /// Gets or sets the settings shell.
        /// </summary>
        public ShellSettings ShellSettings { get; set; }

        /// <summary>
        /// Gets or sets the site name.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the administrator username.
        /// </summary>
        public string AdminUsername { get; set; }

        /// <summary>
        /// Gets or sets the administrator email.
        /// </summary>
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets the administrator password.
        /// </summary>
        public string AdminPassword { get; set; }

        /// <summary>
        /// Gets or sets the database provider.
        /// </summary>
        public string DatabaseProvider { get; set; }

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the database table prefix.
        /// </summary>
        public string DatabaseTablePrefix { get; set; }

        /// <summary>
        /// Gets or sets the time zone for the site.
        /// </summary>
        public string SiteTimeZone { get; set; }

        /// <summary>
        /// Gets or sets the features that will be enable when the set up finished.
        /// </summary>
        public IEnumerable<string> EnabledFeatures { get; set; }

        /// <summary>
        /// Gets or sets the site recipe.
        /// </summary>
        public RecipeDescriptor Recipe { get; set; }

        /// <summary>
        /// Gets or sets the errors during set up if there is.
        /// </summary>
        public IDictionary<string, string> Errors { get; set; }
    }
}
