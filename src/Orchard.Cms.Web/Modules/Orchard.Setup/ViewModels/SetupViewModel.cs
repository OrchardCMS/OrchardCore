using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Orchard.Recipes.Models;
using Orchard.Setup.Annotations;

namespace Orchard.Setup.ViewModels
{
    public class SetupViewModel
    {
        [Required]
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }

        [Required]
        public string DatabaseProvider { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        [Required]
        public string AdminUserName { get; set; }

        [EmailAddress]
        public string AdminEmail { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public IEnumerable<DatabaseProviderEntry> DatabaseProviders { get; set; } = Enumerable.Empty<DatabaseProviderEntry>();

        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public string RecipeName { get; set; }
    }

    public class DatabaseProviderEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool HasConnectionString { get; set; }
    }
}