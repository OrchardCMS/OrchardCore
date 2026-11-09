using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Notifications;

public class NotificationServiceTests
{
    [Fact]
    public async Task SendAsync_MessageWithoutNewProperties_SendsHtmlThroughEmailAndSms()
    {
        var email = new RecordingProvider("Email");
        var sms = new RecordingProvider("SMS");
        var (service, session) = CreateService(email, sms);

        var message = new NotificationMessage
        {
            Subject = "Content published",
            Summary = "<strong>Your post</strong> was published.",
            TextBody = "Your post was published.",
            HtmlBody = "<p>Your <em>post</em> was published.</p>",
            IsHtmlPreferred = true,
        };

        var result = await service.SendAsync(new User { UserId = "user1", UserName = "user1" }, message, TestContext.Current.CancellationToken);

        Assert.Equal(NotificationSendStatus.Success, result.Status);
        Assert.Equal(2, result.SuccessfulCount);
        Assert.Same(message, Assert.Single(email.Messages));
        Assert.Same(message, Assert.Single(sms.Messages));

        // The stored notification keeps the HTML summary shown in the navbar and the notification center.
        Assert.Equal("<strong>Your post</strong> was published.", result.Notification.Summary);
        Assert.Null(result.Notification.Type);
        Assert.Null(result.Notification.Url);
        session.Verify(s => s.SaveAsync(result.Notification, false, NotificationConstants.NotificationCollection, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_TypeTurningSmsOffByDefault_StillSendsEmail()
    {
        var email = new RecordingProvider("Email");
        var sms = new RecordingProvider("SMS");
        var (service, _) = CreateService(email, sms);

        await service.SendAsync(new User { UserId = "user1", UserName = "user1" }, new NotificationMessage { Subject = "Mentioned", Type = "Mention" }, TestContext.Current.CancellationToken);

        Assert.Single(email.Messages);
        Assert.Empty(sms.Messages);
    }

    private static (NotificationService Service, Mock<YesSql.ISession> Session) CreateService(params INotificationMethodProvider[] providers)
    {
        var typeOptions = new NotificationTypeOptions();
        typeOptions.Add("Mention", LocalizationSource.Create("Mentions", typeof(NotificationServiceTests)), type => type.DisabledMethods.Add("SMS"));

        var session = new Mock<YesSql.ISession>();
        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(new DateTime(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc));

        var service = new NotificationService(
            new NotificationMethodProviderAccessor(providers, Options.Create(typeOptions)),
            [],
            NullLogger<NotificationService>.Instance,
            session.Object,
            clock.Object);

        return (service, session);
    }

    private sealed class RecordingProvider : INotificationMethodProvider
    {
        public RecordingProvider(string method)
        {
            Method = method;
        }

        public List<INotificationMessage> Messages { get; } = [];

        public string Method { get; }

        public LocalizedString Name => new(Method, Method);

        public Task<Result> SendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);

            return Task.FromResult(Result.Success());
        }
    }
}
