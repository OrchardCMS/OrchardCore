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
        public static TenantServicesBuilder AddShellDescriptorStorage(this TenantServicesBuilder builder)
        {
            builder.Services.AddScoped<IShellDescriptorManager, ShellDescriptorManager>();
            builder.Services.AddScoped<IShellStateManager, ShellStateManager>();
            builder.Services.AddScoped<IShellFeaturesManager, ShellFeaturesManager>();
            builder.Services.AddScoped<IShellDescriptorFeaturesManager, ShellDescriptorFeaturesManager>();

            return builder;
        }
    }
}