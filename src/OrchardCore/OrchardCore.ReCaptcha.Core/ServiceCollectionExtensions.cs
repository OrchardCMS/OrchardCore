using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptcha.ActionFilters.Detection;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.Services;
using Polly;

namespace OrchardCore.ReCaptcha.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReCaptcha(this IServiceCollection services, Action<ReCaptchaSettings> configure = null)
        {
            services.AddHttpClient<ReCaptchaClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));

            services.AddTransient<IDetectRobots, IpAddressRobotDetector>();
            services.AddTransient<IConfigureOptions<ReCaptchaSettings>, ReCaptchaSettingsConfiguration>();
            services.AddTransient<ReCaptchaService>();

            if (configure != null)
            {
                services.Configure(configure);
            }

            return services;
        }
    }
}
