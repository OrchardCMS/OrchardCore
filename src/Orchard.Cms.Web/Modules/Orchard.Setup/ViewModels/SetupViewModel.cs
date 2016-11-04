using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Orchard.Data;
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
        public bool DatabaseProviderPreset { get; set; }

        public string ConnectionString { get; set; }
        public bool ConnectionStringPreset { get; set; }

        public string TablePrefix { get; set; }
        public bool TablePrefixPreset { get; set; }

        [Required]
        public string AdminUserName { get; set; }

        [EmailAddress]
        public string AdminEmail { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public IEnumerable<DatabaseProvider> DatabaseProviders { get; set; } = Enumerable.Empty<DatabaseProvider>();

        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public string RecipeName { get; set; }
    }
}