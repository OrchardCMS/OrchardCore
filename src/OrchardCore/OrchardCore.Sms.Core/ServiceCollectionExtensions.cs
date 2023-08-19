using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services)
    {
        services.AddTransient<IPostConfigureOptions<SmsSettings>, SmsSettingsConfiguration>();
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<SmsSettings>>().Value;

            if (!String.IsNullOrEmpty(settings.DefaultProviderName))
            {
                var smsProviderOptions = sp.GetRequiredService<IOptions<SmsProviderOptions>>().Value;

                if (smsProviderOptions.Providers.TryGetValue(settings.DefaultProviderName, out var providerGetter))
                {
                    return providerGetter(sp);
                }
            }

            return sp.CreateInstance<DefaultSmsProvider>();
        });

        return services;
    }

    public static IServiceCollection AddPhoneFormatValidator(this IServiceCollection services)
    {
        services.TryAddScoped<IPhoneFormatValidator, DefaultPhoneFormatValidator>();

        return services;
    }

    public static IServiceCollection AddSmsProvider<T>(this IServiceCollection services, string name)
        where T : class, ISmsProvider
    {
        services.Configure<SmsProviderOptions>(options =>
        {
            options.Providers.Add(name, (sp) => sp.CreateInstance<T>());
        });

        return services;
    }

    public static IServiceCollection AddTwilioSmsProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<TwilioSmsProvider>(TwilioSmsProvider.Name);
    }

    public static IServiceCollection AddConsoleSmsProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<ConsoleSmsProvider>(ConsoleSmsProvider.Name);
    }
}
