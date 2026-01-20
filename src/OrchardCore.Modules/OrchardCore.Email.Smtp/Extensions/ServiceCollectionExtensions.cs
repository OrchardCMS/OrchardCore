using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Core;
using OrchardCore.Email.Smtp.Services;

namespace OrchardCore.Email.Smtp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmtpEmailProvider(this IServiceCollection services)
    {
        services.AddEmailProviderOptionsConfiguration<SmtpProviderOptionsConfigurations>();

        return services;
    }
}
