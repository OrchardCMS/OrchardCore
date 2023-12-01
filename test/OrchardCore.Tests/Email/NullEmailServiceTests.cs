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
        var emailService = new NullEmailService(
            emailOptions,
            new FakeLogger<NullEmailService>(),
            Mock.Of<IStringLocalizer<NullEmailService>>(),
            Mock.Of<IEmailAddressValidator>());
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
    }
}
