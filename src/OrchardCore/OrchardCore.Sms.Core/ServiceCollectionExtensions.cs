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
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<ISmsProviderResolver, SmsProviderResolver>();
        services.AddTransient<IPostConfigureOptions<SmsSettings>, SmsSettingsConfiguration>();

        return services;
    }

    public static void AddPhoneFormatValidator(this IServiceCollection services)
        => services.TryAddScoped<IPhoneFormatValidator, DefaultPhoneFormatValidator>();

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
        services.AddHttpClient<TwilioSmsProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.twilio.com/2010-04-01/Accounts/");
        }).AddStandardResilienceHandler();

        services.AddSmsProvider<TwilioSmsProvider>(TwilioSmsProvider.TechnicalName);

        return services;
    }

    public static IServiceCollection AddLogSmsProvider(this IServiceCollection services)
        => services.AddSmsProvider<LogSmsProvider>(LogSmsProvider.TechnicalName);
}
