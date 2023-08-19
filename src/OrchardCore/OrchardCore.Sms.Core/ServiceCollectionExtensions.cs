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
        services.AddSingleton(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SmsSettings>>().Value;

            if (!String.IsNullOrEmpty(settings.DefaultProviderName))
            {
                var smsProviderOptions = serviceProvider.GetRequiredService<IOptions<SmsProviderOptions>>().Value;

                if (smsProviderOptions.Providers.TryGetValue(settings.DefaultProviderName, out var providerGetter))
                {
                    return providerGetter(serviceProvider);
                }
            }

            return serviceProvider.CreateInstance<DefaultSmsProvider>();
        });

        return services;
    }

    public static void AddPhoneFormatValidator(this IServiceCollection services)
    {
        services.TryAddScoped<IPhoneFormatValidator, DefaultPhoneFormatValidator>();
    }

    public static void AddSmsProvider<T>(this IServiceCollection services, string name) where T : class, ISmsProvider
    {
        services.Configure<SmsProviderOptions>(options =>
        {
            options.Providers.TryAdd(name, (sp) => sp.CreateInstance<T>());
        });
    }

    public static IServiceCollection AddTwilioSmsProvider(this IServiceCollection services)
    {
        services.AddSmsProvider<TwilioSmsProvider>(TwilioSmsProvider.Name);

        return services;
    }

    public static IServiceCollection AddConsoleSmsProvider(this IServiceCollection services)
    {
        services.AddSmsProvider<ConsoleSmsProvider>(ConsoleSmsProvider.Name);

        return services;
    }
}
