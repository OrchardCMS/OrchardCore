using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailMessageValidator, EmailMessageValidator>();
        services.AddEmailDeliveryService<NullEmailDeliveryService>(EmailConstants.NullEmailDeliveryServiceName);
        services.AddScoped<IEmailDeliveryServiceResolver, EmailDeliveryServiceResolver>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddEmailDeliveryService<TEmailDeliveryService>(this IServiceCollection services, string key)
        where TEmailDeliveryService : class, IEmailDeliveryService
    {
        services.AddKeyedScoped<IEmailDeliveryService, TEmailDeliveryService>(key);

        return services;
    }
}
