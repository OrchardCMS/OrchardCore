using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Notifications;

public class NotificationManagerTests
{
    [Fact]
    public async Task SendAsync_WithTypeReferenceAndUrl_StoresAndFindsThem()
    {
        using var context = await CreateContextAsync();
        string notificationId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var message = new NotificationMessage
            {
                Subject = "You were mentioned",
                Summary = "Alice mentioned you in a comment.",
                Type = "Mention",
                ReferenceId = "comment-1",
                Url = "~/blog/post-1#comment-1",
            };
            message.Put(new MentionInfo { Author = "Alice" });

            var result = await notificationService.SendAsync(CreateUser("user1"), message, TestContext.Current.CancellationToken);

            notificationId = result.Notification.NotificationId;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

            var notifications = await manager.ListAsync(new NotificationFilter { Type = "Mention", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken);

            var notification = Assert.Single(notifications);
            Assert.Equal(notificationId, notification.NotificationId);
            Assert.Equal("user1", notification.UserId);
            Assert.Equal("~/blog/post-1#comment-1", notification.Url);
            Assert.Equal("Alice", notification.GetOrCreate<MentionInfo>().Author);
            Assert.False(manager.IsRead(notification));
        });
    }

    [Fact]
    public async Task SendAsync_UnsafeUrl_IsNotStored()
    {
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var result = await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage
            {
                Subject = "Unsafe",
                Url = "javascript:alert(1)",
            }, TestContext.Current.CancellationToken);

            Assert.Null(result.Notification.Url);
        });
    }

    [Fact]
    public async Task SendAsync_TypeTooLong_Throws()
    {
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await Assert.ThrowsAsync<ArgumentException>(() => notificationService.SendAsync(CreateUser("user1"), new NotificationMessage
            {
                Subject = "Too long",
                Type = new string('a', NotificationConstants.NotificationTypeLength + 1),
            }, TestContext.Current.CancellationToken));
        });
    }

    [Fact]
    public async Task DeleteAsync_ByTypeAndReference_DeletesOnlyThoseNotifications()
    {
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage { Subject = "1", Type = "Mention", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken);
            await notificationService.SendAsync(CreateUser("user2"), new NotificationMessage { Subject = "2", Type = "Mention", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken);
            await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage { Subject = "3", Type = "Mention", ReferenceId = "comment-2" }, TestContext.Current.CancellationToken);
            await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage { Subject = "4" }, TestContext.Current.CancellationToken);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

            Assert.Equal(2, await manager.DeleteAsync(new NotificationFilter { Type = "Mention", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

            var remaining = await manager.ListAsync(new NotificationFilter { UserId = "user1" }, TestContext.Current.CancellationToken);

            Assert.Equal(["3", "4"], remaining.Select(n => n.Subject).Order());
        });
    }

    [Fact]
    public async Task MarkAsReadAsync_ByFilter_MarksOnlyMatchingUnreadNotifications()
    {
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage { Subject = "1", Type = "Mention", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken);
            await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage { Subject = "2", Type = "Approval", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

            Assert.Equal(1, await manager.MarkAsReadAsync(new NotificationFilter { UserId = "user1", Type = "Mention" }, TestContext.Current.CancellationToken));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

            var unread = await manager.ListAsync(new NotificationFilter { UserId = "user1", IsRead = false }, TestContext.Current.CancellationToken);

            Assert.Equal("Approval", Assert.Single(unread).Type);
        });
    }

    [Fact]
    public async Task UpdateAsync_ChangedSubjectAndUrl_IsSaved()
    {
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.SendAsync(CreateUser("user1"), new NotificationMessage { Subject = "Before", Type = "Mention", ReferenceId = "comment-1" }, TestContext.Current.CancellationToken);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();
            var notification = Assert.Single(await manager.ListAsync(new NotificationFilter { ReferenceId = "comment-1" }, TestContext.Current.CancellationToken));

            notification.Subject = "After";
            notification.Url = "~/comments/1";

            await manager.UpdateAsync(notification, TestContext.Current.CancellationToken);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();
            var notification = Assert.Single(await manager.ListAsync(new NotificationFilter { ReferenceId = "comment-1" }, TestContext.Current.CancellationToken));

            Assert.Equal("After", notification.Subject);
            Assert.Equal("~/comments/1", notification.Url);
        });
    }

    [Fact]
    public async Task ListAsync_EmptyFilter_Throws()
    {
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

            await Assert.ThrowsAsync<ArgumentException>(() => manager.ListAsync(new NotificationFilter { IsRead = false }, TestContext.Current.CancellationToken));
        });
    }

    private static User CreateUser(string userId)
        => new()
        {
            UserId = userId,
            UserName = userId,
        };

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext().WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

            await shellFeaturesManager.EnableFeaturesAsync(availableFeatures.Where(feature => feature.Id == "OrchardCore.Notifications"), force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        return context;
    }

    public sealed class MentionInfo
    {
        public string Author { get; set; }
    }
}
