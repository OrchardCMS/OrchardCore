using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Core;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;

namespace OrchardCore.Email.Smtp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmtpEmailProvider(this IServiceCollection services)
    {
#pragma warning disable CS0612 // Type or member is obsolete
        services.AddSmtpService();
#pragma warning restore CS0612 // Type or member is obsolete

        services.AddEmailProviderOptionsConfiguration<SmtpProviderOptionsConfigurations>();

        return services;
    }

    [Obsolete]
    private static void AddSmtpService(this IServiceCollection services)
    {
        services.AddScoped<ISmtpService, SmtpService>();
    }
}
