using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailMessageValidator, EmailMessageValidator>();
        services.AddScoped<IEmailDeliveryService, NullEmailDeliveryService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
