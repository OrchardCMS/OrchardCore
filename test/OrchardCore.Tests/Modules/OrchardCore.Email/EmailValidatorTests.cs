using OrchardCore.Email;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp;
using OrchardCore.Email.Smtp.Services;

namespace OrchardCore.Modules.Email.Smtp.Tests;

public class EmailValidatorTests
{
    [Fact]
    public async Task SendEmail_WithoutToAndCcAndBccHeaders_ShouldReturnErrors()
    {
        // Arrange
        var message = new MailMessage
        {
            Subject = "Test",
            Body = "Test Message"
        };
        var settings = new SmtpEmailSettings
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
        };

        var smtp = CreateSmtpEmailDeliveryService(settings);

        // Act
        var result = await smtp.DeliverAsync(message);

        // Assert
        Assert.True(result.Errors.Any());
        Assert.NotEqual(EmailResult.Success, result);
    }

    private static SmtpEmailDeliveryService CreateSmtpEmailDeliveryService(SmtpEmailSettings settings) => new(
        Options.Create(settings),
        Mock.Of<ILogger<SmtpEmailDeliveryService>>(),
        Mock.Of<IStringLocalizer<SmtpEmailDeliveryService>>()
    );
}
