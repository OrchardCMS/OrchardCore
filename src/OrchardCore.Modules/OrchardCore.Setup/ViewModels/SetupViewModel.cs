using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Data;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Annotations;

namespace OrchardCore.Setup.ViewModels
{
    public class SetupViewModel
    {
        [Required]
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }

        public string Description { get; set; }

        public DatabaseProviderName? DatabaseProvider { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        /// <summary>
        /// True if the database configuration is preset and can't be changed or displayed on the Setup screen.
        /// </summary>
        [BindNever]
        public bool DatabaseConfigurationPreset { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        [BindNever]
        public IEnumerable<DatabaseProvider> DatabaseProviders { get; set; } = Enumerable.Empty<DatabaseProvider>();

        [BindNever]
        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public bool RecipeNamePreset { get; set; }

        public string RecipeName { get; set; }

        public string SiteTimeZone { get; set; }

        public string Secret { get; set; }
    }
}
