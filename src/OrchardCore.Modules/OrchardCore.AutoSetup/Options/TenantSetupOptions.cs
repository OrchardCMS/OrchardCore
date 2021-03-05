using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OrchardCore.Environment.Shell;

namespace OrchardCore.AutoSetup.Options
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// The tenant setup options.
    /// </summary>
    public class TenantSetupOptions
    {
        /// <summary>
        /// The Shell Name
        /// </summary>
        public string ShellName { get; set; }

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
        /// Gets the Flag which indicates a Default/Root shell/tenant.
        /// </summary>
        public bool IsDefault => string.Equals(ShellName, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Error Message Format
        /// </summary>
        private string RequiredErrorMessageFormat = "The {0} field is required.";

        /// <summary>
        /// Tenant validation.
        /// </summary>
        /// <param name="validationContext"> The validation context. </param>
        /// <returns> The collection of errors. </returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(ShellName) || !Regex.IsMatch(ShellName, @"^\w+$"))
            {
                yield return new ValidationResult("ShellName Can not be empty and must contain characters only and no spaces.");
            }

            if (!IsDefault && string.IsNullOrWhiteSpace(RequestUrlPrefix) && string.IsNullOrWhiteSpace(RequestUrlHost))
            {
                yield return new ValidationResult("RequestUrlPrefix or RequestUrlHost should be provided for no Default Tenant");
            }

            if (!string.IsNullOrEmpty(RequestUrlPrefix) && RequestUrlPrefix.Contains('/'))
            {
                yield return new ValidationResult("The RequestUrlPrefix can not contain more than one segment.");
            }

            if (string.IsNullOrWhiteSpace(SiteName))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(SiteName)));
            }

            if (string.IsNullOrWhiteSpace(AdminUsername))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(AdminUsername)));
            }

            if (string.IsNullOrWhiteSpace(AdminEmail))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(AdminEmail)));
            }

            if (string.IsNullOrWhiteSpace(AdminPassword))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(AdminPassword)));
            }

            if (string.IsNullOrWhiteSpace(DatabaseProvider))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(DatabaseProvider)));
            }

            if (!string.Equals(this.DatabaseProvider, "Sqlite", StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrWhiteSpace(DatabaseConnectionString))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(DatabaseConnectionString)));
            }

            if (string.IsNullOrWhiteSpace(RecipeName))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(RecipeName)));
            }

            if (string.IsNullOrWhiteSpace(SiteTimeZone))
            {
                yield return new ValidationResult(string.Format(RequiredErrorMessageFormat, nameof(SiteTimeZone)));
            }
        }
    }
}
