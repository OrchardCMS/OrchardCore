namespace OrchardCore.AutoSetup
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// The auto setup options.
    /// </summary>
    public class AutoSetupOptions : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the site name.
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
        /// Gets or sets the Url which will trigger AutoSetup.
        /// Leave it Empty if you want to Trigger Setup on any request
        /// </summary>
        public string TriggerSetupUrl { get; set; }

        /// <summary>
        /// Gets or sets the site time zone.
        /// </summary>
        public string SiteTimeZone { get; set; }

        /// <summary>
        /// Options Validation logic.
        /// </summary>
        /// <param name="validationContext">
        /// The validation context.
        /// </param>
        /// <returns>
        /// The collection of validation items <see cref="ValidationResult"/>.
        /// </returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var T = validationContext.GetService<IStringLocalizer<AutoSetupOptions>>();

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

            if (!string.IsNullOrWhiteSpace(TriggerSetupUrl) && !TriggerSetupUrl.StartsWith("/"))
            {
                yield return new ValidationResult(T["The field {0} should be empty or start with /", "Trigger Setup Url"], new[] { nameof(TriggerSetupUrl) });
            }
        }
    }
}
