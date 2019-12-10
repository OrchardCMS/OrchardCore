using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email;
using OrchardCore.Email.Services;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Email
{
    public class EmailTests
    {
        [Fact]
        public async Task SendEmail_WithDisplayName()
        {
            var pickupDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Email");

            if (Directory.Exists(pickupDirectoryPath))
            {
                var directory = new DirectoryInfo(pickupDirectoryPath);
                directory.GetFiles().ToList().ForEach(f => f.Delete());
            }

            Directory.CreateDirectory(pickupDirectoryPath);

            var options = new Mock<IOptions<SmtpSettings>>();
            options.Setup(o => o.Value).Returns(new SmtpSettings {
                DefaultSender = "Your Name <youraddress@host.com>",
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryPath
            });
            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(options.Object, logger.Object, localizer.Object);
            var message = new MailMessage {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            var result = await smtp.SendAsync(message);

            Assert.True(result.Succeeded);
        }
    }
}
