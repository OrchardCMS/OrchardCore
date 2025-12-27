using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ReCaptcha.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureReCaptchaSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore.ReCaptcha");

            tenantServices.PostConfigure<ReCaptchaSettings>(settings => configurationSection.Bind(settings));
        });

        return builder;
    }
}
