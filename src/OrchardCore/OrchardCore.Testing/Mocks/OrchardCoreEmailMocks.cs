using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email;
using OrchardCore.Email.Services;

namespace OrchardCore.Testing.Mocks;

public static partial class OrchardCoreMock
{
    public static ISmtpService CreateSmtpService(SmtpSettings settings)
    {
        var smtpSettings = new Mock<IOptions<SmtpSettings>>();
        smtpSettings
            .Setup(o => o.Value)
            .Returns(settings);

        var logger = new Mock<ILogger<SmtpService>>();
        var localizer = new Mock<IStringLocalizer<SmtpService>>();

        return new SmtpService(smtpSettings.Object, logger.Object, localizer.Object);
    }
}
