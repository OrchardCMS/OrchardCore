using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrchardCore.AutoSetup.Options
{
    /// <summary>
    /// The auto setup options.
    /// </summary>
    public class AutoSetupOptions : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the Url which will trigger AutoSetup.
        /// Leave it Empty if you want to Trigger Setup on any request.
        /// </summary>
        public string AutoSetupPath { get; set; }

        /// <summary>
        /// Gets or sets the Tenants to install.
        /// </summary>
        public List<TenantSetupOptions> Tenants { get; set; } = new List<TenantSetupOptions>();

        /// <summary>
        /// Whether the configuration section exists.
        /// </summary>
        public bool ConfigurationExists { get; set; }

        /// <summary>
        /// AutoSetupOptions Validation logic.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>The collection of errors.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!String.IsNullOrWhiteSpace(AutoSetupPath) && !AutoSetupPath.StartsWith("/"))
            {
                yield return new ValidationResult($"The field {nameof(AutoSetupPath)} should be empty or start with /");
            }

            if (Tenants.Count == 0)
            {
                yield return new ValidationResult($"The field {nameof(Tenants)} should contain at least one tenant");
            }

            if (Tenants.Count(tenant => tenant.IsDefault) != 1)
            {
                yield return new ValidationResult("The Single Default Tenant should be provided");
            }

            foreach (var validationResult in Tenants.SelectMany(tenant => tenant.Validate(validationContext)))
            {
                yield return validationResult;
            }
        }
    }
}
