using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrchardCore.Email.Smtp.HealthChecks;

public class SmtpHealthCheck : IHealthCheck
{
    private readonly SmtpOptions _smtpOptions;

    public SmtpHealthCheck(SmtpOptions smtpOptions) => _smtpOptions = smtpOptions;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var secureSocketOptions = SecureSocketOptions.Auto;

            if (!_smtpOptions.AutoSelectEncryption)
            {
                secureSocketOptions = _smtpOptions.EncryptionMethod switch
                {
                    SmtpEncryptionMethod.None => SecureSocketOptions.None,
                    SmtpEncryptionMethod.SslTls => SecureSocketOptions.SslOnConnect,
                    SmtpEncryptionMethod.StartTls => SecureSocketOptions.StartTls,
                    _ => SecureSocketOptions.Auto,
                };
            }

            using var client = new SmtpClient();

            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, secureSocketOptions, CancellationToken.None);

            if (!client.IsConnected)
            {
                return HealthCheckResult.Unhealthy(description: $"The client is not connected to the SMTP server.");
            }

            if (_smtpOptions.RequireCredentials)
            {
                var username = _smtpOptions.UserName;
                var password = _smtpOptions.Password;
                if (_smtpOptions.UseDefaultCredentials)
                {
                    username = password = string.Empty;
                }

                await client.AuthenticateAsync(username, password, CancellationToken.None);

                if (!client.IsAuthenticated)
                {
                    return HealthCheckResult.Unhealthy(description: $"The client is not authenticated with the SMTP server.");
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Retrieving the status of the SMTP service failed.", ex);
        }
    }
}
