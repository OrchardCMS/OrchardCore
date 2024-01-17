using MimeKit;
using OrchardCore.Email;
using OrchardCore.Email.Services;

namespace OrchardCore.Tests.Email
{
    public class EmailTests
    {
        [Fact]
        public async Task SendEmail_WithToHeader()
        {
            // Arrange
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            // Act
            var content = await SendEmailAsync(message, "Your Name <youraddress@host.com>");

            // Assert
            Assert.Contains("From: Your Name <youraddress@host.com>", content);
        }

        [Fact]
        public async Task SendEmail_WithCcHeader()
        {
            // Arrange
            var message = new MailMessage
            {
                Cc = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            // Act
            var content = await SendEmailAsync(message);

            // Assert
            Assert.Contains("Cc: info@oc.com", content);
        }

        [Fact]
        public async Task SendEmail_WithBccHeader()
        {
            // Arrange
            var message = new MailMessage
            {
                Bcc = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            // Act
            var content = await SendEmailAsync(message);

            // Assert
            Assert.Contains("Bcc: info@oc.com", content);
        }

        [Fact]
        public async Task SendEmail_WithDisplayName()
        {
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };

            await SendEmailAsync(message, "Your Name <youraddress@host.com>");
        }

        [Fact]
        public async Task SendEmail_UsesDefaultSender()
        {
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };
            var content = await SendEmailAsync(message, "Your Name <youraddress@host.com>");

            Assert.Contains("From: Your Name <youraddress@host.com>", content);
        }

        [Fact]
        public async Task SendEmail_UsesCustomSender()
        {
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message",
                From = "My Name <youraddress@host.com>",
            };
            var content = await SendEmailAsync(message, "Your Name <youraddress@host.com>");

            Assert.Contains("From: My Name <youraddress@host.com>", content);
            Assert.Contains("Sender: Your Name <youraddress@host.com>", content);
        }

        [Fact]
        public async Task SendEmail_UsesCustomAuthorAndSender()
        {
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message",
                Sender = "Hisham Bin Ateya <hishamco_2007@hotmail.com>",
            };
            var content = await SendEmailAsync(message, "Sebastien Ros <sebastienros@gmail.com>");

            Assert.Contains("From: Sebastien Ros <sebastienros@gmail.com>", content);
            Assert.Contains("Sender: Hisham Bin Ateya <hishamco_2007@hotmail.com>", content);
        }

        [Fact]
        public async Task SendEmail_UsesMultipleAuthors()
        {
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message",
                From = "sebastienros@gmail.com,hishamco_2007@hotmail.com"
            };
            var content = await SendEmailAsync(message, "Hisham Bin Ateya <hishamco_2007@hotmail.com>");

            Assert.Contains("From: sebastienros@gmail.com, hishamco_2007@hotmail.com", content);
            Assert.Contains("Sender: Hisham Bin Ateya <hishamco_2007@hotmail.com>", content);
        }

        [Fact]
        public async Task SendEmail_UsesReplyTo()
        {
            var message = new MailMessage
            {
                To = "Hisham Bin Ateya <hishamco_2007@hotmail.com>",
                Subject = "Test",
                Body = "Test Message",
                From = "Hisham Bin Ateya <hishamco_2007@hotmail.com>",
                ReplyTo = "Hisham Bin Ateya <hishamco_2007@yahoo.com>",
            };
            var content = await SendEmailAsync(message, "Your Name <youraddress@host.com>");

            Assert.Contains("From: Hisham Bin Ateya <hishamco_2007@hotmail.com>", content);
            Assert.Contains("Reply-To: Hisham Bin Ateya <hishamco_2007@yahoo.com>", content);
        }

        [Fact]
        public async Task ReplyTo_ShouldHaveAuthors_IfNotSet()
        {
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message",
                From = "Sebastien Ros <sebastienros@gmail.com>"
            };
            var content = await SendEmailAsync(message, "Your Name <youraddress@host.com>");

            Assert.Contains("From: Sebastien Ros <sebastienros@gmail.com>", content);
            Assert.Contains("Reply-To: Sebastien Ros <sebastienros@gmail.com>", content);
        }

        [Theory]
        [InlineData("me <mailbox@domain.com>", "me", "mailbox@domain.com")]
        [InlineData("me<mailbox@domain.com>", "me", "mailbox@domain.com")]
        [InlineData("me     <mailbox@domain.com>", "me", "mailbox@domain.com")]
        [InlineData("<mailbox@domain.com>", "", "mailbox@domain.com")]
        [InlineData("mailbox@domain.com", "", "mailbox@domain.com")]
        [InlineData("(comment)mailbox(comment)@(comment)domain.com(me) ", "me", "mailbox@domain.com")]
        [InlineData("Sébastien <sébastien@domain.com>", "Sébastien", "sébastien@domain.com")]
        public void MailBoxAddress_ShouldParseEmail(string text, string name, string address)
        {
            Assert.True(MailboxAddress.TryParse(text, out var mailboxAddress));
            Assert.Equal(name, mailboxAddress.Name);
            Assert.Equal(address, mailboxAddress.Address);
        }

        [Fact]
        public async Task SendEmail_WithoutToAndCcAndBccHeaders_ShouldThrowsException()
        {
            // Arrange
            var message = new MailMessage
            {
                Subject = "Test",
                Body = "Test Message"
            };
            var settings = new SmtpSettings
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            };

            var smtp = CreateSmtpService(settings);

            // Act
            var result = await smtp.SendAsync(message);

            // Assert
            Assert.True(result.Errors.Any());
        }

        [Fact]
        public async Task SendOfflineEmailHasNoResponse()
        {
            // Arrange
            var message = new MailMessage
            {
                To = "info@oc.com",
                Subject = "Test",
                Body = "Test Message"
            };
            var settings = new SmtpSettings
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            };

            var smtp = CreateSmtpService(settings);

            // Act
            var result = await smtp.SendAsync(message);

            // Assert
            Assert.Null(result.Response);
        }

        private static async Task<string> SendEmailAsync(MailMessage message, string defaultSender = null)
        {
            var pickupDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Email");

            if (Directory.Exists(pickupDirectoryPath))
            {
                var directory = new DirectoryInfo(pickupDirectoryPath);
                directory.GetFiles().ToList().ForEach(f => f.Delete());
            }

            Directory.CreateDirectory(pickupDirectoryPath);

            var settings = new SmtpSettings
            {
                DefaultSender = defaultSender,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryPath
            };
            var smtp = CreateSmtpService(settings);

            var result = await smtp.SendAsync(message);

            Assert.True(result.Succeeded);

            var file = new DirectoryInfo(pickupDirectoryPath).GetFiles().FirstOrDefault();

            Assert.NotNull(file);

            var content = File.ReadAllText(file.FullName);

            return content;
        }

        private static ISmtpService CreateSmtpService(SmtpSettings settings)
        {
            var options = new Mock<IOptions<SmtpSettings>>();
            options.Setup(o => o.Value).Returns(settings);

            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(options.Object, logger.Object, localizer.Object);

            return smtp;
        }
    }
}
