using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Sms.Azure.Services;

namespace OrchardCore.Sms.Azure;

public static class SmsProviderExtensions
{
    public static IServiceCollection AddAzureSmsProvider(this IServiceCollection services)
        => services.AddSmsProviderOptionsConfiguration<AzureSmsProviderOptionsConfigurations>();
}
