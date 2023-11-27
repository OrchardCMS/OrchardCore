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
        services.AddTransient<IPostConfigureOptions<SmsSettings>, SmsSettingsConfiguration>();

        services.AddHttpClient<TwilioSmsProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.twilio.com/2010-04-01/Accounts/");
        }).AddStandardResilienceHandler();

        services.AddScoped<ISmsService, SmsService>();

        return services;
    }

    public static void AddPhoneFormatValidator(this IServiceCollection services)
        => services.TryAddScoped<IPhoneFormatValidator, DefaultPhoneFormatValidator>();

    public static IServiceCollection AddSmsProvider<T>(this IServiceCollection services, string name) where T : class, ISmsProvider
        => services.AddKeyedScoped<ISmsProvider, T>(name);

    public static IServiceCollection AddTwilioSmsProvider(this IServiceCollection services)
        => services.AddKeyedScoped<ISmsProvider, TwilioSmsProvider>(TwilioSmsProvider.TechnicalName);

    public static IServiceCollection AddLogSmsProvider(this IServiceCollection services)
        => services.AddKeyedScoped<ISmsProvider, LogSmsProvider>(LogSmsProvider.TechnicalName);
}
