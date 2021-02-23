using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
        /// Leave it Empty if you want to Trigger Setup on any request
        /// </summary>
        public string AutoSetupPath { get; set; }

        /// <summary>
        /// Gets or sets the Tenants to install.
        /// </summary>
        public List<TenantSetupOptions> Tenants { get; set; } = new List<TenantSetupOptions>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var T = validationContext.GetService<IStringLocalizer<AutoSetupOptions>>();

            if (!string.IsNullOrWhiteSpace(AutoSetupPath) && !AutoSetupPath.StartsWith("/"))
            {
                yield return new ValidationResult(T["The field {0} should be empty or start with /", "Auto Setup Path"], new[] { nameof(AutoSetupPath) });
            }

            if (Tenants == null || Tenants.Count == 0)
            {
                yield return new ValidationResult(T["The field {0} should contain at least one tenant", "Tenants"], new[] { nameof(Tenants) });
            }

            if (Tenants.Count(tenant => tenant.IsDefault) != 1)
            {
                yield return new ValidationResult(T["The Single Default Tenant should be provided", "Tenants"], new[] { nameof(Tenants) });
            }

            foreach (var tenant in Tenants)
            {
                foreach (var validationResult in tenant.Validate(validationContext))
                {
                    yield return validationResult;
                }
            }
        }
    }
}
