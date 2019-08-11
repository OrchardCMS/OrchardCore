using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using System.Collections.Generic;

namespace OrchardCore.Setup.Services
{
    public class SetupContext
    {
        public ShellSettings ShellSettings { get; set; }
        public string SiteName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public string DatabaseProvider { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public string SiteTimeZone { get; set; }
        public IEnumerable<string> EnabledFeatures { get; set; }
        public RecipeDescriptor Recipe { get; set; }
        public IDictionary<string, string> Errors { get; set; }
    }
}