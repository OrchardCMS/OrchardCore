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
        public static IServiceCollection AddReCaptcha(this IServiceCollection services)
        {
            services.AddHttpClient<IReCaptchaClient, ReCaptchaClient>(client =>
                {
                    const string noCaptchaUrl = Constants.ReCaptchaApiUri;
                    client.BaseAddress = new Uri(noCaptchaUrl);
                })
                .AddTransientHttpErrorPolicy(policy => 
                    policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));

            services.AddTransient<IDetectAbuse, IpAddressAbuseDetector>();
            services.AddTransient<IConfigureOptions<ReCaptchaSettings>, ReCaptchaSettingsConfiguration>();

            return services;
        }
    }
}
