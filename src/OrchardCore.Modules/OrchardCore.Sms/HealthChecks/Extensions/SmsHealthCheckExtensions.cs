using OrchardCore.Sms.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class SmsHealthCheckExtensions
{
    public static IHealthChecksBuilder AddSmsCheck(this IHealthChecksBuilder healthChecksBuilder)
        => healthChecksBuilder.AddCheck<SmsHealthCheck>("SMS Health Check");
}
