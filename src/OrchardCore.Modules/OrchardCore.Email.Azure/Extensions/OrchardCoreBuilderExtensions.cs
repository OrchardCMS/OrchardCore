using Microsoft.Extensions.Configuration;
using OrchardCore.Email.Azure;
using OrchardCore.Environment.Shell.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureAzureEmailSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Email_Azure");

                tenantServices.PostConfigure<AzureEmailSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }
    }
}
