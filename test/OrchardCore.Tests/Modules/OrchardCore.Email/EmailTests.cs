using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
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
            options.Setup(o => o.Value).Returns(new SmtpSettings
            {
                DefaultSender = "Your Name <youraddress@host.com>",
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryPath
            });
            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(options.Object, logger.Object, localizer.Object);
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            var result = await smtp.SendAsync(message);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SendEmail_UsesDefaultSender()
        {
            var pickupDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Email");

            if (Directory.Exists(pickupDirectoryPath))
            {
                var directory = new DirectoryInfo(pickupDirectoryPath);
                directory.GetFiles().ToList().ForEach(f => f.Delete());
            }

            Directory.CreateDirectory(pickupDirectoryPath);

            var options = new Mock<IOptions<SmtpSettings>>();
            options.Setup(o => o.Value).Returns(new SmtpSettings
            {
                DefaultSender = "Your Name <youraddress@host.com>",
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryPath
            });
            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(options.Object, logger.Object, localizer.Object);
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            var result = await smtp.SendAsync(message);

            Assert.True(result.Succeeded);

            var file = new DirectoryInfo(pickupDirectoryPath).GetFiles().FirstOrDefault();

            Assert.NotNull(file);

            var content = File.ReadAllText(file.FullName);

            Assert.Contains("From: Your Name <youraddress@host.com>", content);
        }

        [Fact]
        public async Task SendEmail_UsesCustomSender()
        {
            var pickupDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Email");

            if (Directory.Exists(pickupDirectoryPath))
            {
                var directory = new DirectoryInfo(pickupDirectoryPath);
                directory.GetFiles().ToList().ForEach(f => f.Delete());
            }

            Directory.CreateDirectory(pickupDirectoryPath);

            var options = new Mock<IOptions<SmtpSettings>>();
            options.Setup(o => o.Value).Returns(new SmtpSettings
            {
                DefaultSender = "Your Name <youraddress@host.com>",
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryPath
            });
            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(options.Object, logger.Object, localizer.Object);
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message",
                From = "My Name <youraddress@host.com>",
            };

            var result = await smtp.SendAsync(message);

            Assert.True(result.Succeeded);

            var file = new DirectoryInfo(pickupDirectoryPath).GetFiles().FirstOrDefault();

            Assert.NotNull(file);

            var content = File.ReadAllText(file.FullName);

            Assert.Contains("From: My Name <youraddress@host.com>", content);
            Assert.Contains("Sender: My Name <youraddress@host.com>", content);
        }

        [Fact]
        public async Task SendEmail_UsesReplyTo()
        {
            var pickupDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Email");

            if (Directory.Exists(pickupDirectoryPath))
            {
                var directory = new DirectoryInfo(pickupDirectoryPath);
                directory.GetFiles().ToList().ForEach(f => f.Delete());
            }

            Directory.CreateDirectory(pickupDirectoryPath);

            var options = new Mock<IOptions<SmtpSettings>>();
            options.Setup(o => o.Value).Returns(new SmtpSettings
            {
                DefaultSender = "Your Name <youraddress@host.com>",
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryPath
            });
            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(options.Object, logger.Object, localizer.Object);
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message",
                From = "sebastienros@gmail.com",
                ReplyTo = "sebastienros@gmail.com",
            };

            var result = await smtp.SendAsync(message);

            Assert.True(result.Succeeded);

            var file = new DirectoryInfo(pickupDirectoryPath).GetFiles().FirstOrDefault();

            Assert.NotNull(file);

            var content = File.ReadAllText(file.FullName);

            Assert.Contains("From: sebastienros@gmail.com", content);
        }

        [Theory]
        [InlineData("me <mailbox@domain.com>", "me", "mailbox@domain.com")]
        [InlineData("me<mailbox@domain.com>", "me", "mailbox@domain.com")]
        [InlineData("me     <mailbox@domain.com>", "me", "mailbox@domain.com")]
        [InlineData("<mailbox@domain.com>", "", "mailbox@domain.com")]
        [InlineData("mailbox@domain.com", "", "mailbox@domain.com")]
        [InlineData("(comment)mailbox(comment)@(comment)domain.com(me) ", "me", "mailbox@domain.com")]
        public void MailBoxAddress_ShouldParseEmail(string text, string name, string address)
        {
            Assert.True(MailboxAddress.TryParse(text, out var mailboxAddress));

            Assert.Equal(name, mailboxAddress.Name);
            Assert.Equal(address, mailboxAddress.Address);
        }
    }
}
