using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;

namespace OrchardCore.AutoSetup.Options
{
    /// <summary>
    /// The tenant setup options.
    /// </summary>
    public class TenantSetupOptions
    {
        private readonly string _requiredErrorMessageFormat = "The {0} field is required.";

        private bool? _isDefault;

        /// <summary>
        /// The Shell Name.
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
        /// Gets or sets the database's schema.
        /// </summary>
        public string DatabaseSchema { get; set; }

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
        /// Gets or sets the name of the feature profile applied to the tenant. May be <see langword="null"/> or empty
        /// for no profile being selected.
        /// </summary>
        public string FeatureProfile { get; set; }

        /// <summary>
        /// Gets the Flag which indicates a Default/Root shell/tenant.
        /// </summary>
        public bool IsDefault => _isDefault ??= ShellName.IsDefaultShellName();

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

            if (!IsDefault && ShellName.IsDefaultShellNameIgnoreCase())
            {
                yield return new ValidationResult("The tenant name is in conflict with the 'Default' tenant name.");
            }

            if (!String.IsNullOrWhiteSpace(RequestUrlPrefix) && RequestUrlPrefix.Contains('/'))
            {
                yield return new ValidationResult("The RequestUrlPrefix can not contain more than one segment.");
            }

            if (String.IsNullOrWhiteSpace(SiteName))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(SiteName)));
            }

            if (String.IsNullOrWhiteSpace(AdminUsername))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(AdminUsername)));
            }

            if (String.IsNullOrWhiteSpace(AdminEmail))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(AdminEmail)));
            }

            if (String.IsNullOrWhiteSpace(AdminPassword))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(AdminPassword)));
            }

            var selectedProvider = validationContext.GetServices<DatabaseProvider>().FirstOrDefault(x => x.Value == DatabaseProvider);
            if (selectedProvider == null)
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(DatabaseProvider)));
            }

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(DatabaseConnectionString))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(DatabaseConnectionString)));
            }

            if (String.IsNullOrWhiteSpace(RecipeName))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(RecipeName)));
            }

            if (String.IsNullOrWhiteSpace(SiteTimeZone))
            {
                yield return new ValidationResult(String.Format(_requiredErrorMessageFormat, nameof(SiteTimeZone)));
            }
        }
    }
}
