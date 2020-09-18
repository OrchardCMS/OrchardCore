using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Setup.Options
{
    public class AutoSetupOptions : IValidatableObject
    {
        public string SiteName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        //public bool CreateDatabase { get; set; }
        public string DatabaseProvider { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public string RecipeName { get; set; }
        public string SiteTimeZone { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var T = validationContext.GetService<IStringLocalizer<AutoSetupOptions>>();

            if (String.IsNullOrWhiteSpace(SiteName))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Site name"], new[] { nameof(SiteName) });
            }

            if (String.IsNullOrWhiteSpace(AdminUsername))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Admin UserName"], new[] { nameof(AdminUsername) });
            }

            if (String.IsNullOrWhiteSpace(AdminEmail))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Admin EmailAddress"], new[] { nameof(AdminEmail) });
            }

            if (String.IsNullOrWhiteSpace(AdminPassword))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Admin Password"], new[] { nameof(AdminPassword) });
            }

            if (String.IsNullOrWhiteSpace(DatabaseProvider))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Database Provider"], new[] { nameof(DatabaseProvider) });
            }

            if (String.IsNullOrWhiteSpace(DatabaseConnectionString))
            { 
                yield return new ValidationResult(T["The field {0} is not provided", "Database ConnectionString"], new[] { nameof(DatabaseConnectionString) });
            }

            if (String.IsNullOrWhiteSpace(DatabaseTablePrefix))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Database TablePrefix"], new[] { nameof(DatabaseTablePrefix) });
            }

            if (String.IsNullOrWhiteSpace(RecipeName))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Recipe name"], new[] { nameof(RecipeName) });
            }

            if (String.IsNullOrWhiteSpace(SiteTimeZone))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Site TimeZone"], new[] { nameof(SiteTimeZone) });
            }

            //if(CreateDatabase && String.IsNullOrWhiteSpace(DatabaseName))
            //{
            //    yield return new ValidationResult(T["The field {0} is not provided", "DatabaseName"], new[] { nameof(DatabaseName) });
            //}
        }
    }
}
