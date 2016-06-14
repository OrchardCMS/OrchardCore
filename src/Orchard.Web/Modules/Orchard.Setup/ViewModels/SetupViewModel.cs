using System.ComponentModel.DataAnnotations;
using Orchard.Setup.Annotations;
using System.Collections.Generic;
using Orchard.Recipes.Models;

namespace Orchard.Setup.ViewModels
{
    public class SetupViewModel
    {
        [Required]
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }

        [Required]
        public string DatabaseProvider { get; set; }

        [Required]
        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        [Required]
        public string AdminUserName { get; set; }

        [EmailAddress]
        public string AdminEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordConfirmation { get; set; }

        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        [Required]
        public string RecipeName { get; set; }

        public string RecipeDescription { get; set; }
    }
}