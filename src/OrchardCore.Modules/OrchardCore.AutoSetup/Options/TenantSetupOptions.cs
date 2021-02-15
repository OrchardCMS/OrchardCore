using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AutoSetup.Options
{
    /// <summary>
    /// The tenant setup options.
    /// </summary>
    public class TenantSetupOptions: BaseTenantSetupOptions
    {
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
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var T = validationContext.GetService<IStringLocalizer<AutoSetupOptions>>();

            if (string.IsNullOrWhiteSpace(RequestUrlPrefix) && string.IsNullOrWhiteSpace(RequestUrlHost))
            {
                yield return new ValidationResult(T["Either Tenant's RequestUrlPrefix or RequestUrlHost should be provided", "Tenant Url"], new[] { "TenantUrl" });
            }

            foreach (var validationResult in base.Validate(validationContext))
            {
                yield return validationResult;
            }
        }
    }
}
