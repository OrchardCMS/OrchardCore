using System.ComponentModel.DataAnnotations;

namespace Orchard.Tenants.ViewModels
{
    public class EditTenantViewModel
    {
        public string SiteName { get; set; }

        public string DatabaseProvider { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string AdminUserName { get; set; }

        [EmailAddress]
        public string AdminEmail { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        //public IEnumerable<DatabaseProviderEntry> DatabaseProviders { get; set; } = Enumerable.Empty<DatabaseProviderEntry>();

        //public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public string RecipeName { get; set; }
    }
}
