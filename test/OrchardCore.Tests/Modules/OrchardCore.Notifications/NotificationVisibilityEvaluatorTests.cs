using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
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
    public async Task FilterAsync_HandlerAlwaysThrows_HidesAllWithLimitedCalls()
    {
        var calls = 0;
        var logger = new TestLogger();
        var evaluator = new NotificationVisibilityEvaluator(
            [
                new BatchHandler(_ =>
                {
                    calls++;

                    throw new InvalidOperationException();
                }),
            ],
            logger);

        var visible = await evaluator.FilterAsync(CreateNotifications(100), _user, TestContext.Current.CancellationToken);

        Assert.Empty(visible);
        Assert.Equal(9, calls);
        Assert.True(evaluator.HasFailures);
        Assert.Equal(LogLevel.Error, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public async Task FilterAsync_HandlerThrowsForOneAmongMany_HidesItWithFewCallsAndLogsItsId()
    {
        var calls = 0;
        var logger = new TestLogger();
        var evaluator = new NotificationVisibilityEvaluator(
            [
                new BatchHandler(contexts =>
                {
                    calls++;

                    if (contexts.Any(c => c.Notification.NotificationId == "73"))
                    {
                        throw new InvalidOperationException();
                    }
                }),
            ],
            logger);

        var visible = await evaluator.FilterAsync(CreateNotifications(100), _user, TestContext.Current.CancellationToken);

        Assert.Equal(99, visible.Count);
        Assert.DoesNotContain(visible, n => n.NotificationId == "73");
        Assert.InRange(calls, 2, 15);
        Assert.Contains("73", Assert.Single(logger.Entries).Message);
    }

    [Fact]
    public async Task FilterAsync_HandlerThrowsOnce_HidesNothing()
    {
        var calls = 0;
        var logger = new TestLogger();
        var evaluator = new NotificationVisibilityEvaluator(
            [
                new BatchHandler(_ =>
                {
                    if (calls++ == 0)
                    {
                        throw new InvalidOperationException();
                    }
                }),
            ],
            logger);

        var visible = await evaluator.FilterAsync(CreateNotifications(4), _user, TestContext.Current.CancellationToken);

        Assert.Equal(4, visible.Count);
        Assert.False(evaluator.HasFailures);
        Assert.Equal(LogLevel.Warning, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public async Task FilterAsync_HandlerThrowsAfterEarlierHandlerHidAll_DoesNotCallItAgain()
    {
        var calls = 0;
        var evaluator = new NotificationVisibilityEvaluator(
            [
                new DelegateHandler(context => context.Hide()),
                new BatchHandler(_ =>
                {
                    calls++;

                    throw new InvalidOperationException();
                }),
            ],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        var visible = await evaluator.FilterAsync(CreateNotifications(50), _user, TestContext.Current.CancellationToken);

        Assert.Empty(visible);
        Assert.Equal(1, calls);
        Assert.False(evaluator.HasFailures);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task FilterAsync_Cancelled_ThrowsWithoutKeepingADecision(int count)
    {
        using var source = new CancellationTokenSource();
        var cancelled = false;
        var logger = new TestLogger();
        var evaluator = new NotificationVisibilityEvaluator(
            [
                new BatchHandler(_ =>
                {
                    if (!cancelled)
                    {
                        cancelled = true;
                        source.Cancel();
                        source.Token.ThrowIfCancellationRequested();
                    }
                }),
            ],
            logger);

        var notifications = CreateNotifications(count);

        await Assert.ThrowsAsync<OperationCanceledException>(() => evaluator.FilterAsync(notifications, _user, source.Token));

        Assert.Empty(logger.Entries);
        Assert.False(evaluator.HasFailures);
        Assert.Equal(count, (await evaluator.FilterAsync(notifications, _user, TestContext.Current.CancellationToken)).Count);
    }

    [Fact]
    public async Task FilterAsync_HandlerThrowsForOneNotification_HidesOnlyThatNotification()
    {
        var evaluator = new NotificationVisibilityEvaluator(
            [
                new BatchHandler(contexts =>
                {
                    if (contexts.Any(c => c.Notification.ReferenceId == "broken"))
                    {
                        throw new InvalidOperationException();
                    }
                }),
                new DelegateHandler(context =>
                {
                    if (context.Notification.Type == "Mention")
                    {
                        context.Restrict();
                    }
                }),
            ],
            NullLogger<NotificationVisibilityEvaluator>.Instance);

        var notifications = new[] { CreateNotification("1", null), CreateNotification("2", "Mention", "broken"), CreateNotification("3", "Mention") };

        var visible = await evaluator.FilterAsync(notifications, _user, TestContext.Current.CancellationToken);

        Assert.Equal(["1", "3"], visible.Select(n => n.NotificationId));
        Assert.Equal(NotificationVisibility.Restricted, await evaluator.EvaluateAsync(notifications[2], _user, TestContext.Current.CancellationToken));
    }

    private static List<Notification> CreateNotifications(int count)
        => Enumerable.Range(1, count).Select(i => CreateNotification(i.ToString(CultureInfo.InvariantCulture), null)).ToList();

    private static Notification CreateNotification(string id, string type, string referenceId = null)
        => new()
        {
            NotificationId = id,
            Type = type,
            ReferenceId = referenceId,
            UserId = "user1",
        };

    private sealed class TestLogger : ILogger<NotificationVisibilityEvaluator>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }

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
