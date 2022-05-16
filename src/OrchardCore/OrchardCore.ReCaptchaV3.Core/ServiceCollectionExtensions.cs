using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptchaV3.Configuration;
using OrchardCore.ReCaptchaV3.Services;
using Polly;

namespace OrchardCore.ReCaptchaV3.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReCaptchaV3(this IServiceCollection services, Action<ReCaptchaV3Settings> configure = null)
        {
            services.AddHttpClient<ReCaptchaV3Client>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));

            services.AddTransient<IConfigureOptions<ReCaptchaV3Settings>, ReCaptchaV3SettingsConfiguration>();
            services.AddTransient<ReCaptchaV3Service>();

            if (configure != null)
            {
                services.Configure(configure);
            }

            return services;
        }
    }
}
