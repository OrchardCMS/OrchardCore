using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Data;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Annotations;
using YesSql;

namespace OrchardCore.Setup.ViewModels
{
    public class SetupViewModel
    {
        [Required]
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }

        public string Description { get; set; }

        public string DatabaseProvider { get; set; }

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

        [RegularExpression("^_*$", ErrorMessage = "Invalid table name separator")]
        public string TableNameSeparator { get; set; }

        [RegularExpression("^[A-Za-z_]+[A-Za-z0-9_]*$", ErrorMessage = "Invalid schema")]
        public string Schema { get; set; }

        [RegularExpression("^[A-Za-z_]+[A-Za-z0-9_]*$", ErrorMessage = "Invalid name")]
        public string DocumentTable { get; set; }

        public IdentityColumnSize IdentityColumnSize { get; set; }
    }
}
