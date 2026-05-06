using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.Extensions;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureReCaptchaSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_ReCaptcha");

            tenantServices.PostConfigure<ReCaptchaSettings>(settings => configurationSection.Bind(settings));
        });

        return builder;
    }
}
