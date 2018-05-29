using OrchardCore.Environment.Shell.Descriptor.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        /// <summary>
        /// Registers at the tenant level a set of features which are always enabled for this tenant.
        /// </summary>
        public static TenantServicesBuilder AddEnabledFeatures(this TenantServicesBuilder tenant, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                tenant.Services.AddTransient(sp => new ShellFeature(featureId, alwaysEnabled: true));
            }

            return tenant;
        }
    }
}
