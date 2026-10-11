using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.BackgroundTasks;
using OrchardCore.BackgroundTasks.Controllers;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Documents;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace OrchardCore.Tests.Modules.OrchardCore.BackgroundTasks;

public class BackgroundTaskControllerTests
{
    private const string Tenant = "Default";

    private static readonly string _firstTaskName = typeof(FirstTestBackgroundTask).FullName;
    private static readonly string _secondTaskName = typeof(SecondTestBackgroundTask).FullName;

    [Fact]
    public async Task Run_IdleTask_QueuesRunAndRedirectsToReturnUrl()
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        monitor
            .Setup(x => x.RequestRunAsync(Tenant, _firstTaskName))
            .ReturnsAsync(BackgroundTaskRunRequestResult.Queued);

        var notifier = new TestNotifier();
        var controller = CreateController(monitor.Object, notifier);

        var result = await controller.Run(_firstTaskName, "/Admin/BackgroundTasks/Details/Task");

        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/Admin/BackgroundTasks/Details/Task", redirectResult.Url);
        monitor.Verify(x => x.RequestRunAsync(Tenant, _firstTaskName), Times.Once);

        var entry = Assert.Single(notifier.Entries);
        Assert.Equal(NotifyType.Success, entry.Type);
        Assert.Contains("First test task", entry.Message.Value);
    }

    [Fact]
    public async Task Run_ExternalReturnUrl_RedirectsToIndex()
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        monitor
            .Setup(x => x.RequestRunAsync(Tenant, _firstTaskName))
            .ReturnsAsync(BackgroundTaskRunRequestResult.Queued);

        var controller = CreateController(monitor.Object, new TestNotifier());

        var result = await controller.Run(_firstTaskName, "https://example.com/");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BackgroundTaskController.Index), redirectResult.ActionName);
    }

    [Theory]
    [InlineData(BackgroundTaskRunRequestResult.AlreadyRunning, NotifyType.Warning)]
    [InlineData(BackgroundTaskRunRequestResult.AlreadyQueued, NotifyType.Information)]
    [InlineData(BackgroundTaskRunRequestResult.NotFound, NotifyType.Warning)]
    public async Task Run_TaskNotQueued_NotifiesWhy(BackgroundTaskRunRequestResult requestResult, NotifyType expectedType)
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        monitor
            .Setup(x => x.RequestRunAsync(Tenant, _firstTaskName))
            .ReturnsAsync(requestResult);

        var notifier = new TestNotifier();
        var controller = CreateController(monitor.Object, notifier);

        await controller.Run(_firstTaskName, null);

        Assert.Equal(expectedType, Assert.Single(notifier.Entries).Type);
    }

    [Fact]
    public async Task Run_UnknownTask_ReturnsNotFound()
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        var controller = CreateController(monitor.Object, new TestNotifier());

        var result = await controller.Run("Unknown.Task", null);

        Assert.IsType<NotFoundResult>(result);
        monitor.Verify(x => x.RequestRunAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_WithoutMonitor_NotifiesError()
    {
        var notifier = new TestNotifier();
        var controller = CreateController(monitor: null, notifier);

        var result = await controller.Run(_firstTaskName, null);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(NotifyType.Error, Assert.Single(notifier.Entries).Type);
    }

    [Fact]
    public async Task Run_Unauthorized_ReturnsForbid()
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        var controller = CreateController(monitor.Object, new TestNotifier(), authorized: false);

        var result = await controller.Run(_firstTaskName, null);

        Assert.IsType<ForbidResult>(result);
        monitor.Verify(x => x.RequestRunAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IndexBulkActionPOST_Run_QueuesSelectedTasks()
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        monitor
            .Setup(x => x.RequestRunAsync(Tenant, It.IsAny<string>()))
            .ReturnsAsync(BackgroundTaskRunRequestResult.Queued);

        var notifier = new TestNotifier();
        var controller = CreateController(monitor.Object, notifier);

        var result = await controller.IndexBulkActionPOST(
            new AdminIndexOptions { BulkAction = BackgroundTaskBulkAction.Run },
            [_firstTaskName, _secondTaskName, "Unknown.Task"]);

        Assert.IsType<RedirectToActionResult>(result);
        monitor.Verify(x => x.RequestRunAsync(Tenant, _firstTaskName), Times.Once);
        monitor.Verify(x => x.RequestRunAsync(Tenant, _secondTaskName), Times.Once);
        monitor.Verify(x => x.RequestRunAsync(Tenant, "Unknown.Task"), Times.Never);
        Assert.Equal(2, notifier.Entries.Count(entry => entry.Type == NotifyType.Success));
    }

    [Fact]
    public async Task Index_Default_IncludesExecutionStates()
    {
        var firstState = new BackgroundTaskState { Name = _firstTaskName, Tenant = Tenant, Status = BackgroundTaskStatus.Running };
        var controller = CreateController(CreateMonitor(firstState).Object, new TestNotifier());

        var result = await controller.Index(new AdminIndexOptions(), new PagerParameters());

        var model = Assert.IsType<BackgroundTaskIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.True(model.IsMonitored);
        Assert.Equal(2, model.Tasks.Count);
        Assert.Same(firstState, model.Tasks.Single(task => task.Name == _firstTaskName).State);
        Assert.Null(model.Tasks.Single(task => task.Name == _secondTaskName).State);
        Assert.Contains(model.Options.BulkActions, action => action.Value == nameof(BackgroundTaskBulkAction.Run));
        Assert.Equal("*/10 * * * *", model.Tasks.Single(task => task.Name == _firstTaskName).Schedule);
    }

    [Fact]
    public async Task Index_FailedStatusFilter_ListsTasksWhoseLastRunFailed()
    {
        var monitor = CreateMonitor(
            new BackgroundTaskState
            {
                Name = _firstTaskName,
                Tenant = Tenant,
                LastRun = new BackgroundTaskRun { Result = BackgroundTaskRunResult.Failed, ErrorMessage = "Boom" },
            },
            new BackgroundTaskState
            {
                Name = _secondTaskName,
                Tenant = Tenant,
                LastRun = new BackgroundTaskRun { Result = BackgroundTaskRunResult.Succeeded },
            });

        var controller = CreateController(monitor.Object, new TestNotifier());

        var result = await controller.Index(new AdminIndexOptions { Status = "failed" }, new PagerParameters());

        var model = Assert.IsType<BackgroundTaskIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(_firstTaskName, Assert.Single(model.Tasks).Name);
    }

    [Fact]
    public async Task Index_RunningStatusFilter_ListsQueuedAndRunningTasks()
    {
        var monitor = CreateMonitor(
            new BackgroundTaskState { Name = _firstTaskName, Tenant = Tenant, Status = BackgroundTaskStatus.Queued },
            new BackgroundTaskState { Name = _secondTaskName, Tenant = Tenant, Status = BackgroundTaskStatus.Idle });

        var controller = CreateController(monitor.Object, new TestNotifier());

        var result = await controller.Index(new AdminIndexOptions { Status = "running" }, new PagerParameters());

        var model = Assert.IsType<BackgroundTaskIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(_firstTaskName, Assert.Single(model.Tasks).Name);
    }

    [Fact]
    public async Task Index_WithoutMonitor_HidesRunActions()
    {
        var controller = CreateController(monitor: null, new TestNotifier());

        var result = await controller.Index(new AdminIndexOptions(), new PagerParameters());

        var model = Assert.IsType<BackgroundTaskIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.False(model.IsMonitored);
        Assert.All(model.Tasks, task => Assert.Null(task.State));
        Assert.DoesNotContain(model.Options.BulkActions, action => action.Value == nameof(BackgroundTaskBulkAction.Run));
        Assert.DoesNotContain(model.Options.Statuses, status => status.Value == "failed");
    }

    [Fact]
    public async Task Details_KnownTask_IncludesSettingsAndState()
    {
        var state = new BackgroundTaskState { Name = _firstTaskName, Tenant = Tenant, RunCount = 3 };
        var monitor = new Mock<IBackgroundTaskMonitor>();
        monitor
            .Setup(x => x.GetStateAsync(Tenant, _firstTaskName))
            .ReturnsAsync(state);

        var controller = CreateController(monitor.Object, new TestNotifier());

        var result = await controller.Details(_firstTaskName);

        var model = Assert.IsType<BackgroundTaskDetailsViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(_firstTaskName, model.Name);
        Assert.Equal("First test task", model.Title);
        Assert.Equal("*/10 * * * *", model.Schedule);
        Assert.True(model.IsMonitored);
        Assert.Same(state, model.State);
    }

    [Fact]
    public async Task Details_UnknownTask_ReturnsNotFound()
    {
        var controller = CreateController(new Mock<IBackgroundTaskMonitor>().Object, new TestNotifier());

        var result = await controller.Details("Unknown.Task");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task States_Default_ReturnsStatusOfTenantTasks()
    {
        var lastStartTime = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var monitor = CreateMonitor(
            new BackgroundTaskState { Name = _firstTaskName, Tenant = Tenant, Status = BackgroundTaskStatus.Running, LastStartTime = lastStartTime, RunCount = 4 });

        var controller = CreateController(monitor.Object, new TestNotifier());

        var result = await controller.States();

        var json = JsonSerializer.SerializeToElement(Assert.IsType<JsonResult>(result).Value);
        var state = Assert.Single(json.EnumerateArray());
        Assert.Equal(_firstTaskName, state.GetProperty("Name").GetString());
        Assert.Equal("Running", state.GetProperty("Status").GetString());
        Assert.Equal(lastStartTime, state.GetProperty("LastStartTime").GetDateTime());
        Assert.Equal(4, state.GetProperty("RunCount").GetInt64());
    }

    private static Mock<IBackgroundTaskMonitor> CreateMonitor(params BackgroundTaskState[] states)
    {
        var monitor = new Mock<IBackgroundTaskMonitor>();
        monitor
            .Setup(x => x.GetStatesAsync(Tenant))
            .ReturnsAsync(states);

        return monitor;
    }

    private static BackgroundTaskController CreateController(IBackgroundTaskMonitor monitor, INotifier notifier, bool authorized = true)
    {
        var document = new BackgroundTaskDocument();
        document.Settings[_firstTaskName] = new BackgroundTaskSettings
        {
            Name = _firstTaskName,
            Title = "First test task",
            Schedule = "*/10 * * * *",
        };

        var documentManager = new Mock<IDocumentManager<BackgroundTaskDocument>>();
        documentManager
            .Setup(x => x.GetOrCreateImmutableAsync(It.IsAny<Func<Task<BackgroundTaskDocument>>>()))
            .ReturnsAsync(document);
        documentManager
            .Setup(x => x.GetOrCreateMutableAsync(It.IsAny<Func<Task<BackgroundTaskDocument>>>()))
            .ReturnsAsync(document);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        // Calls the default implementation of the generic overload, which calls the setup below.
        var shapeFactory = new Mock<IShapeFactory> { CallBase = true };
        shapeFactory
            .Setup(x => x.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<ValueTask<IShape>>>(),
                It.IsAny<Action<ShapeCreatingContext>>(),
                It.IsAny<Action<ShapeCreatedContext>>()))
            .Returns(() => ValueTask.FromResult<IShape>(new Shape()));

        var htmlLocalizer = new Mock<IHtmlLocalizer<BackgroundTaskController>>();
        htmlLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedHtmlString(name, name));
        htmlLocalizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedHtmlString(name, string.Format(CultureInfo.InvariantCulture, name, arguments), false, arguments));

        var stringLocalizer = new Mock<IStringLocalizer<BackgroundTaskController>>();
        stringLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(x => x.IsLocalUrl(It.IsAny<string>()))
            .Returns((string url) => !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal));

        return new BackgroundTaskController(
            authorizationService.Object,
            [new FirstTestBackgroundTask(), new SecondTestBackgroundTask()],
            new BackgroundTaskManager(documentManager.Object, Mock.Of<ISignal>()),
            monitor is null ? [] : [monitor],
            new ShellSettings { Name = Tenant },
            Options.Create(new PagerOptions()),
            shapeFactory.Object,
            htmlLocalizer.Object,
            stringLocalizer.Object,
            notifier)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "TestAuth")),
                },
            },
            Url = urlHelper.Object,
        };
    }

    [BackgroundTask(Title = "First test task", Schedule = "* * * * *")]
    private sealed class FirstTestBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [BackgroundTask(Title = "Second test task", Schedule = "0 0 * * *")]
    private sealed class SecondTestBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestNotifier : INotifier
    {
        public List<Notification> Entries { get; } = [];

        public ValueTask AddAsync(NotifyType type, LocalizedHtmlString message)
        {
            Entries.Add(new Notification(type, message));

            return ValueTask.CompletedTask;
        }

        public IList<NotifyEntry> List()
            => Entries.Select(entry => new NotifyEntry { Type = entry.Type, Message = entry.Message }).ToList();
    }

    private sealed record Notification(NotifyType Type, LocalizedHtmlString Message);
}
