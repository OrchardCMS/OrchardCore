using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AutoSetup.Options
{
    /// <summary>
    /// The tenant setup options.
    /// </summary>
    public class TenantSetupOptions
    {
        /// <summary>
        /// Gets or sets the Flag which indicates a Default/Root shell/tenant.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the user friendly Tenant/Site Name.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the admin username.
        /// </summary>
        public string AdminUsername { get; set; }

        /// <summary>
        /// Gets or sets the admin email.
        /// </summary>
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets the admin password.
        /// </summary>
        public string AdminPassword { get; set; }

        /// <summary>
        /// Gets or sets the database provider.
        /// </summary>
        public string DatabaseProvider { get; set; }

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the database table prefix.
        /// </summary>
        public string DatabaseTablePrefix { get; set; }

        /// <summary>
        /// Gets or sets the recipe name.
        /// </summary>
        public string RecipeName { get; set; }

        /// <summary>
        /// Gets or sets the site time zone.
        /// </summary>
        public string SiteTimeZone { get; set; }

        /// <summary>
        /// Gets or sets the tenants request url host.
        /// </summary>
        public string RequestUrlHost { get; set; }

        /// <summary>
        /// Gets or sets the tenant request url prefix.
        /// </summary>
        public string RequestUrlPrefix { get; set; }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="validationContext"> The validation context. </param>
        /// <returns>
        /// The collection of errors if any. </returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var T = validationContext.GetService<IStringLocalizer<AutoSetupOptions>>();

            if (!IsDefault && string.IsNullOrWhiteSpace(RequestUrlPrefix) && string.IsNullOrWhiteSpace(RequestUrlHost))
            {
                yield return new ValidationResult(T["For no Default Tenant RequestUrlPrefix or RequestUrlHost should be provided", "Tenant Url"], new[] { "TenantUrl" });
            }

            if (string.IsNullOrWhiteSpace(SiteName))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Site name"], new[] { nameof(SiteName) });
            }

            if (string.IsNullOrWhiteSpace(AdminUsername))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Admin UserName"], new[] { nameof(AdminUsername) });
            }

            if (string.IsNullOrWhiteSpace(AdminEmail))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Admin EmailAddress"], new[] { nameof(AdminEmail) });
            }

            if (string.IsNullOrWhiteSpace(AdminPassword))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Admin Password"], new[] { nameof(AdminPassword) });
            }

            if (string.IsNullOrWhiteSpace(DatabaseProvider))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Database Provider"], new[] { nameof(DatabaseProvider) });
            }

            if (!string.Equals(this.DatabaseProvider, "Sqlite", StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrWhiteSpace(DatabaseConnectionString))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Database ConnectionString"], new[] { nameof(DatabaseConnectionString) });
            }

            if (string.IsNullOrWhiteSpace(RecipeName))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Recipe name"], new[] { nameof(RecipeName) });
            }

            if (string.IsNullOrWhiteSpace(SiteTimeZone))
            {
                yield return new ValidationResult(T["The field {0} is not provided", "Site TimeZone"], new[] { nameof(SiteTimeZone) });
            }
        }
    }
}
