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
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();

            tenantServices.PostConfigure<ReCaptchaSettings>(settings =>
            {
                // The 'OrchardCore_ReCaptcha' section is deprecated and will be removed in a future major version, use 'ReCaptcha' instead.
                configuration.GetSectionCompat("ReCaptcha", "OrchardCore_ReCaptcha").Bind(settings);
            });
        });

        return builder;
    }
}
