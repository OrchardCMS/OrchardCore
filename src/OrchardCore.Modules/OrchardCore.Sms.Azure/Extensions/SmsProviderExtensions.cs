using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Options;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Sms.Azure.Services;

namespace OrchardCore.Sms.Azure;

public static class SmsProviderExtensions
{
    public static IServiceCollection AddAzureSmsProvider(this IServiceCollection services)
        => services.AddSmsProviderOptionsConfiguration<AzureSmsProviderOptionsConfigurations>()
        .AddSignalOptionsChangeTokenSource<AzureSmsOptions>()
        .AddTransient<IConfigureOptions<AzureSmsOptions>, AzureSmsOptionsConfiguration>();
}
