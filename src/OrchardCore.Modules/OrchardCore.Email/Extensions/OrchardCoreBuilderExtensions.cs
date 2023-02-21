using Microsoft.Extensions.Configuration;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureEmailSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Email");

                tenantServices.PostConfigure<SmtpSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }
    }
}
