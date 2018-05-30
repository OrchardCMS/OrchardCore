using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Data.Descriptors;
using OrchardCore.Environment.Shell.Descriptor;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        /// <summary>
        /// Per-tenant services to store shell state and shell descriptors in the database.
        /// </summary>
        public static TenantServicesBuilder AddShellDescriptorStorage(this TenantServicesBuilder tenant)
        {
            var services = tenant.Services;

            services.AddScoped<IShellDescriptorManager, ShellDescriptorManager>();
            services.AddScoped<IShellStateManager, ShellStateManager>();
            services.AddScoped<IShellFeaturesManager, ShellFeaturesManager>();
            services.AddScoped<IShellDescriptorFeaturesManager, ShellDescriptorFeaturesManager>();

            return tenant;
        }
    }
}