using Microsoft.Extensions.Logging.Testing;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Tests;

public class NullEmailServiceTests
{
    [Fact]
    public async Task SendEmail()
    {
        // Arrange
        var emailOptions = Options.Create(new EmailSettings
        {
            DefaultSender = "info@orchardcore.net"
        });
        var stringLocalizer = Mock.Of<IStringLocalizer<EmailServiceBase<EmailSettings>>>();
        var logCollector = FakeLogCollector.Create(new FakeLogCollectorOptions());
        var logger = new FakeLogger<EmailServiceBase<EmailSettings>>(logCollector);
        var emailService = new NullEmailService(emailOptions, logger, stringLocalizer);
        var message = new MailMessage
        {
            To = "hishamco_2007@hotmail.com",
            Subject = "Orchard Core",
            Body = "This is a test message."
        };

        // Act
        var result = await emailService.SendAsync(message);

        // Assert
        Assert.True(result.Succeeded);

        var records = logCollector.GetSnapshot();

        Assert.Equal(4, records.Count);
        Assert.All(records, r => Assert.True(r.Level == LogLevel.Debug));
        Assert.Equal($"From: {message.From}", records[0].Message);
        Assert.Equal($"To: {message.To}", records[1].Message);
        Assert.Equal($"Subject: {message.Subject}", records[2].Message);
        Assert.Equal($"Body: {message.Body}", records[3].Message);
    }
}
