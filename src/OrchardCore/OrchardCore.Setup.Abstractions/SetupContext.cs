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
        /// <summary>
        /// Gets additional key/value info.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
