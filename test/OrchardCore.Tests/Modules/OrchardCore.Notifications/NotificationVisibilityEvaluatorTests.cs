using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Notifications;

public class NotificationVisibilityEvaluatorTests
{
    private static readonly ClaimsPrincipal _user = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user1")], "Test"));

    [Fact]
    public async Task EvaluateAsync_NoHandler_ReturnsVisible()
    {
        var evaluator = new NotificationVisibilityEvaluator([], NullLogger<NotificationVisibilityEvaluator>.Instance);

        Assert.False(evaluator.HasHandlers);
        Assert.Equal(NotificationVisibility.Visible, await evaluator.EvaluateAsync(CreateNotification("1", "Mention"), _user, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EvaluateAsync_HideAndRestrict_MostRestrictiveWins()
    {
        var evaluator = new NotificationVisibilityEvaluator(
            [new DelegateHandler(c => c.Hide()), new DelegateHandler(c => c.Restrict())],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        Assert.Equal(NotificationVisibility.Hidden, await evaluator.EvaluateAsync(CreateNotification("1", "Mention"), _user, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EvaluateAsync_HandlerThrows_ReturnsHidden()
    {
        var evaluator = new NotificationVisibilityEvaluator(
            [new DelegateHandler(_ => throw new InvalidOperationException()), new DelegateHandler(c => c.Restrict())],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        Assert.Equal(NotificationVisibility.Hidden, await evaluator.EvaluateAsync(CreateNotification("1", "Mention"), _user, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EvaluateAsync_SameNotificationTwice_CallsHandlersOnce()
    {
        var calls = 0;
        var evaluator = new NotificationVisibilityEvaluator(
            [new DelegateHandler(_ => calls++)],
            NullLogger<NotificationVisibilityEvaluator>.Instance);
        var notification = CreateNotification("1", "Mention");

        await evaluator.EvaluateAsync(notification, _user, TestContext.Current.CancellationToken);
        await evaluator.EvaluateAsync(notification, _user, TestContext.Current.CancellationToken);

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task FilterAsync_HiddenNotifications_AreRemovedInOrder()
    {
        var evaluator = new NotificationVisibilityEvaluator(
            [new DelegateHandler(c =>
            {
                if (c.Notification.Type == "Mention" && c.Notification.ReferenceId == "secret")
                {
                    c.Hide();
                }
                else if (c.Notification.ReferenceId == "limited")
                {
                    c.Restrict();
                }
            })],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        var visible = await evaluator.FilterAsync(
            [
                CreateNotification("1", "Mention", "public"),
                CreateNotification("2", "Mention", "secret"),
                CreateNotification("3", "Mention", "limited"),
                CreateNotification("4", null),
            ],
            _user,
            TestContext.Current.CancellationToken);

        Assert.Equal(["1", "3", "4"], visible.Select(n => n.NotificationId));
    }

    [Fact]
    public async Task FilterAsync_ManyNotifications_CallsHandlerOnceWithOnlyTheMissingOnes()
    {
        var batches = new List<string[]>();
        var evaluator = new NotificationVisibilityEvaluator(
            [new BatchHandler(contexts => batches.Add(contexts.Select(c => c.Notification.NotificationId).ToArray()))],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        await evaluator.EvaluateAsync(CreateNotification("1", null), _user, TestContext.Current.CancellationToken);
        await evaluator.FilterAsync([CreateNotification("1", null), CreateNotification("2", null), CreateNotification("3", null), CreateNotification("3", null)], _user, TestContext.Current.CancellationToken);
        await evaluator.FilterAsync([CreateNotification("2", null)], _user, TestContext.Current.CancellationToken);

        Assert.Equal([["1"], ["2", "3"]], batches);
    }

    [Fact]
    public async Task FilterAsync_HandlerThrows_HidesTheBatch()
    {
        var evaluator = new NotificationVisibilityEvaluator(
            [new BatchHandler(_ => throw new InvalidOperationException())],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        var visible = await evaluator.FilterAsync([CreateNotification("1", null), CreateNotification("2", null)], _user, TestContext.Current.CancellationToken);

        Assert.Empty(visible);
    }

    private static Notification CreateNotification(string id, string type, string referenceId = null)
        => new()
        {
            NotificationId = id,
            Type = type,
            ReferenceId = referenceId,
            UserId = "user1",
        };

    private sealed class BatchHandler : INotificationVisibilityHandler
    {
        private readonly Action<IReadOnlyList<NotificationVisibilityContext>> _evaluate;

        public BatchHandler(Action<IReadOnlyList<NotificationVisibilityContext>> evaluate)
        {
            _evaluate = evaluate;
        }

        public Task EvaluateAsync(IReadOnlyList<NotificationVisibilityContext> contexts, CancellationToken cancellationToken = default)
        {
            _evaluate(contexts);

            return Task.CompletedTask;
        }
    }

    private sealed class DelegateHandler : INotificationVisibilityHandler
    {
        private readonly Action<NotificationVisibilityContext> _evaluate;

        public DelegateHandler(Action<NotificationVisibilityContext> evaluate)
        {
            _evaluate = evaluate;
        }

        public Task EvaluateAsync(IReadOnlyList<NotificationVisibilityContext> contexts, CancellationToken cancellationToken = default)
        {
            foreach (var context in contexts)
            {
                _evaluate(context);
            }

            return Task.CompletedTask;
        }
    }
}
