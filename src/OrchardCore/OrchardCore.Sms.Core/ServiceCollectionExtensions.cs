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
        services.AddHttpClient(TwilioSmsProvider.TechnicalName, client =>
        {
            client.BaseAddress = new Uri("https://api.twilio.com/2010-04-01/Accounts/");
        });

        services.AddSingleton(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SmsSettings>>().Value;

            if (!String.IsNullOrEmpty(settings.DefaultProviderName))
            {
                var smsProviderOptions = serviceProvider.GetRequiredService<IOptions<SmsProviderOptions>>().Value;

                if (smsProviderOptions.Providers.TryGetValue(settings.DefaultProviderName, out var type))
                {
                    return serviceProvider.CreateInstance<ISmsProvider>(type);
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

    public static IServiceCollection AddSmsProvider<T>(this IServiceCollection services, string name) where T : class, ISmsProvider
    {
        services.Configure<SmsProviderOptions>(options =>
        {
            options.TryAddProvider(name, typeof(T));
        });

        return services;
    }

    public static IServiceCollection AddTwilioSmsProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<TwilioSmsProvider>(TwilioSmsProvider.TechnicalName);
    }

    public static IServiceCollection AddLogSmsProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<LogSmsProvider>(LogSmsProvider.TechnicalName);
    }
}
