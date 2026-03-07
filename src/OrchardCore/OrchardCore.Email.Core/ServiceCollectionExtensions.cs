using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Configuration;
using OrchardCore.Email.Events;
using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddSingleton<IEmailProviderFactory, EmailProviderFactory>();

        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IEmailServiceEvents, EmailMessageValidator>();
        services.AddTransient<IConfigureOptions<EmailOptions>, EmailOptionsConfiguration>();

        return services;
    }
}
