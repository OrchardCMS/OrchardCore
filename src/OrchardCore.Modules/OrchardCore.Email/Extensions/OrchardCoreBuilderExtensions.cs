using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Email;

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
