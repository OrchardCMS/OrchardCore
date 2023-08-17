using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services)
    {
        services.AddSingleton<DefaultSmsProvider>();
        services.AddTransient<IPostConfigureOptions<SmsSettings>, SmsSettingsConfigurator>();
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

            return sp.GetRequiredService<DefaultSmsProvider>();
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
        services.AddSingleton<T>();
        services.Configure<SmsProviderOptions>(options =>
        {
            options.Providers.Add(name, (sp) => sp.GetService<T>());
        });

        return services;
    }

    public static IServiceCollection AddTwilioProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<TwilioSmsProvider>(SmsConstants.TwilioServiceName);
    }

    public static IServiceCollection AddConsoleProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<ConsoleSmsProvider>(SmsConstants.ConsoleServiceName);
    }
}
