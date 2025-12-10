using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

// MEIJIRO_TODO: Added this
using OrchardCore.ReCaptcha.Settings;

namespace Microsoft.Extensions.DependencyInjection;
public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureReCaptchaSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore.ReCaptcha");

            tenantServices.PostConfigure<ReCaptchaSettings>(settings => shellConfiguration.Bind(settings));
        });

        return builder;
    }
}
