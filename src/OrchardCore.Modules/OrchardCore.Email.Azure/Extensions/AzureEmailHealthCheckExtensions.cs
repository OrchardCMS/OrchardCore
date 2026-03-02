using OrchardCore.Email.Azure.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class AzureEmailHealthCheckExtensions
{
    public static IHealthChecksBuilder AddAzureEmailCheck(this IHealthChecksBuilder healthChecksBuilder)
        => healthChecksBuilder.AddCheck<AzureEmailHealthCheck>("Azure Email Health Check");
}
