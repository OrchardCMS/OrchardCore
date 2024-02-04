using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, DefaultEmailService>();
        services.AddScoped<IEmailProviderResolver, DefaultEmailProviderResolver>();
        services.AddTransient<IPostConfigureOptions<EmailSettings>, EmailSettingsConfiguration>();

        return services;
    }

    public static IServiceCollection AddEmailProvider<T>(this IServiceCollection services, string name)
        where T : class, IEmailProvider
    {
        services.Configure<EmailProviderOptions>(options =>
        {
            options.TryAddProvider(name, new EmailProviderTypeOptions(typeof(T))
            {
                IsEnabled = true,
            });
        });

        return services;
    }

    public static IServiceCollection AddEmailProviderOptionsConfiguration<TConfiguration>(this IServiceCollection services)
        where TConfiguration : class, IConfigureOptions<EmailProviderOptions>
    {
        services.AddTransient<IConfigureOptions<EmailProviderOptions>, TConfiguration>();

        return services;
    }

    public static IServiceCollection AddSmtpEmailProvider(this IServiceCollection services)
    {
        services.AddScoped<ISmtpService, SmtpService>();

        services.AddEmailProviderOptionsConfiguration<SmtpProviderOptionsConfigurations>();

        return services;
    }
}
