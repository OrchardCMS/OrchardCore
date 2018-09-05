using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptcha.Core.ActionFilters.Abuse;
using OrchardCore.ReCaptcha.Core.Configuration;
using OrchardCore.ReCaptcha.Core.Services;
using Polly;

namespace OrchardCore.ReCaptcha.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReCaptcha(this IServiceCollection services, Action<ReCaptchaSettings> configure = null)
        {
            services.AddHttpClient<IReCaptchaClient, ReCaptchaClient>(client =>
            {
                var settings = services
                    .BuildServiceProvider()
                    .GetRequiredService<IOptions<ReCaptchaSettings>>();

                client.BaseAddress = new Uri(settings.Value.ReCaptchaApiUri);
            })
            .AddTransientHttpErrorPolicy(policy => 
                policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));

            services.AddTransient<IDetectAbuse, IpAddressAbuseDetector>();
            services.AddTransient<IConfigureOptions<ReCaptchaSettings>, ReCaptchaSettingsConfiguration>();
            services.AddTransient<IReCaptchaService, ReCaptchaService>();

            if (configure != null)
                services.Configure(configure);

            return services;
        }
    }
}
