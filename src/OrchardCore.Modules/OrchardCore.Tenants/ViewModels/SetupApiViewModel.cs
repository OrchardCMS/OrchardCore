using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using YesSql;

namespace OrchardCore.Tenants.ViewModels
{
    public class SetupApiViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string SiteName { get; set; }

        public string DatabaseProvider { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string RecipeName { get; set; }

        public IFormFile Recipe { get; set; }

        public string SiteTimeZone { get; set; }

        [RegularExpression("^[A-Za-z_]+[A-Za-z0-9_]*$", ErrorMessage = "Invalid schema")]
        public string Schema { get; set; }

        [RegularExpression("^[A-Za-z_]+[A-Za-z0-9_]*$", ErrorMessage = "Invalid name")]
        public string DocumentTable { get; set; }

        [RegularExpression("^_*$|\\bNULL\\b$", ErrorMessage = "Invalid table name separator")]
        public string TableNameSeparator { get; set; }

        public IdentityColumnSize IdentityColumnSize { get; set; }
    }
}
