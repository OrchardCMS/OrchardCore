using OrchardCore.Sms.Azure.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class AzureSmsHealthCheckExtensions
{
    public static IHealthChecksBuilder AddAzureSmsHealthCheck(this IHealthChecksBuilder healthChecksBuilder)
        => healthChecksBuilder.AddCheck<AzureSmsHealthCheck>("Azure SMS Health Check");
}
