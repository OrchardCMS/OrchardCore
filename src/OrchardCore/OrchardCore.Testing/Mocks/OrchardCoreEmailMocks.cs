using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email;
using OrchardCore.Email.Smtp.Services;

namespace OrchardCore.Testing.Mocks;

public static partial class OrchardCoreMock
{
    public static SmtpEmailProvider CreateSmtpService(SmtpOptions smtpOptions)
    {
        var options = new Mock<IOptions<SmtpOptions>>();
        options.Setup(o => o.Value)
            .Returns(smtpOptions);

        var logger = new Mock<ILogger<SmtpEmailProvider>>();
        var localizer = new Mock<IStringLocalizer<SmtpEmailProvider>>();
        var emailValidator = new Mock<IEmailAddressValidator>();

        emailValidator.Setup(x => x.Validate(It.IsAny<string>()))
            .Returns(true);

        var smtp = new SmtpEmailProvider(options.Object, emailValidator.Object, logger.Object, localizer.Object);

        return smtp;
    }
}
