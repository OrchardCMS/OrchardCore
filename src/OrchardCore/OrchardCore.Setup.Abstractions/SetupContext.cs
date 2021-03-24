using System.Collections.Generic;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Services
{
    /// <summary>
    /// Represents a class that contains setup information.
    /// </summary>
    public class SetupContext
    {
        /// <summary>
        /// Gets or sets the shell settings.
        /// </summary>
        public ShellSettings ShellSettings { get; set; }

        /// <summary>
        /// Gets or sets the features that will be enabled.
        /// </summary>
        public IEnumerable<string> EnabledFeatures { get; set; }

        /// <summary>
        /// Gets or sets the site recipe.
        /// </summary>
        public RecipeDescriptor Recipe { get; set; }

        /// <summary>
        /// Gets or sets the errors that occurred during the setup.
        /// </summary>
        public IDictionary<string, string> Errors { get; set; }

        /// <summary>
        /// Gets additional key/value info.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
