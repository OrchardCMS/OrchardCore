using OrchardCore.Email.Smtp.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class SmtpHealthCheckExtensions
{
    public static IHealthChecksBuilder AddSmtpCheck(this IHealthChecksBuilder healthChecksBuilder)
        => healthChecksBuilder.AddCheck<SmtpHealthCheck>("SMTP Health Check");
}
