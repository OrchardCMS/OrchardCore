using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using YesSql;

namespace OrchardCore.Tenants.ViewModels
{
    public class TenantViewModel
    {
        public string Description { get; set; }

        [Required]
        public string Name { get; set; }

        public string DatabaseProvider { get; set; }

        public string RequestUrlPrefix { get; set; }

        public string RequestUrlHost { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string RecipeName { get; set; }

        public string FeatureProfile { get; set; }

        [RegularExpression("^_*$", ErrorMessage = "Invalid table name separator")]
        public string TableNameSeparator { get; set; }

        [RegularExpression("^[A-Za-z_]+[A-Za-z0-9_]*$", ErrorMessage = "Invalid schema")]
        public string Schema { get; set; }

        [RegularExpression("^[A-Za-z_]+[A-Za-z0-9_]*$", ErrorMessage = "Invalid name")]
        public string DocumentTable { get; set; }

        public IdentityColumnSize IdentityColumnType { get; set; }

        [BindNever]
        public bool IsNewTenant { get; set; }
    }
}
