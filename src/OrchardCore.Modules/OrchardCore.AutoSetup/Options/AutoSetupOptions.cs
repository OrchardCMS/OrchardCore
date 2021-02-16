namespace OrchardCore.AutoSetup.Options
{
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
        /// Gets or sets the Url which will trigger AutoSetup.
        /// Leave it Empty if you want to Trigger Setup on any request
        /// </summary>
        public string AutoSetupPath { get; set; }

        /// <summary>
        /// Gets or sets the Root tenant setup options.
        /// </summary>
        public BaseTenantSetupOptions RootTenant { get; set; }

        /// <summary>
        /// Gets or sets the Sub-tenants.
        /// </summary>
        public List<TenantSetupOptions> SubTenants { get; set; } = new List<TenantSetupOptions>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var T = validationContext.GetService<IStringLocalizer<AutoSetupOptions>>();

            if (RootTenant == null)
            {
                yield return new ValidationResult(T["The field {0} should not be null", "Root Tenant"], new[] { nameof(RootTenant) });
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(AutoSetupPath) && !AutoSetupPath.StartsWith("/"))
            {
                yield return new ValidationResult(T["The field {0} should be empty or start with /", "Auto Setup Path"], new[] { nameof(AutoSetupPath) });
            }

            foreach (var validationResult in RootTenant.Validate(validationContext))
            {
                yield return validationResult;
            }

            if (SubTenants != null && SubTenants.Count > 0)
            {
                foreach (var tenant in SubTenants)
                {
                    foreach (var validationResult in tenant.Validate(validationContext))
                    {
                        yield return validationResult;
                    }
                }
            }
        }
    }
}
