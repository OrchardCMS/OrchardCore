using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Options;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services)
    {
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<ISmsProviderResolver, DefaultSmsProviderResolver>();
        services.AddSignalOptionsChangeTokenSource<SmsProviderOptions>();
        services.AddTransient<IPostConfigureOptions<SmsSettings>, SmsSettingsConfiguration>();

        return services;
    }

    public static IServiceCollection AddSmsProvider<T>(this IServiceCollection services, string name)
        where T : class, ISmsProvider
    {
        services.Configure<SmsProviderOptions>(options =>
        {
            options.TryAddProvider(name, new SmsProviderTypeOptions(typeof(T))
            {
                IsEnabled = true,
            });
        });

        return services;
    }

    public static IServiceCollection AddSmsProviderOptionsConfiguration<TConfiguration>(this IServiceCollection services)
        where TConfiguration : class, IConfigureOptions<SmsProviderOptions>
    {
        services.AddTransient<IConfigureOptions<SmsProviderOptions>, TConfiguration>();

        return services;
    }

    public static IServiceCollection AddTwilioSmsProvider(this IServiceCollection services)
    {
        services.AddHttpClient(TwilioSmsProvider.TechnicalName, client =>
        {
            client.BaseAddress = new Uri("https://api.twilio.com/2010-04-01/Accounts/");
        }).AddStandardResilienceHandler();

        return services.AddSignalOptionsChangeTokenSource<TwilioOptions>()
            .AddSmsProviderOptionsConfiguration<TwilioProviderOptionsConfigurations>()
            .AddTransient<IConfigureOptions<TwilioOptions>, TwilioOptionsConfiguration>();
    }

    public static IServiceCollection AddLogSmsProvider(this IServiceCollection services)
        => services.AddSmsProvider<LogSmsProvider>(LogSmsProvider.TechnicalName);
}
