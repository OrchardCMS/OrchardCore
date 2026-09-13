using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using YesSql.Filters.Query;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AuditTrail.Endpoints;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.BackgroundTasks;
using OrchardCore.BackgroundTasks.Controllers;
using OrchardCore.BackgroundTasks.Endpoints;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Documents;
using OrchardCore.Environment.Cache;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class AuditTaskManagementTests
{
    [Fact]
    public async Task AuditSearch_DelegatesTheParsedFiltersAndPageToTheExistingQueryService()
    {
        const string query = "id:content category:Content";
        var parsed = new QueryEngineBuilder<AuditTrailEvent>().Build().Parse(string.Empty);
        var parser = new Mock<IAuditTrailAdminListFilterParser>();
        parser.Setup(value => value.Parse(query)).Returns(parsed);
        var queries = new Mock<IAuditTrailAdminListQueryService>();
        queries.Setup(value => value.QueryAsync(3, 2, It.Is<AuditTrailIndexOptions>(options => options.FilterResult == parsed)))
            .ReturnsAsync(new AuditTrailEventQueryResult { TotalCount = 5, Events = [new AuditTrailEvent { EventId = "event" }] });
        var result = Assert.IsType<Ok<AuditTrailPage>>(await AuditTrailEndpoints.ListAsync(new DefaultHttpContext(), Authorization(), parser.Object, queries.Object, query, 3, 2));
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Page);
        Assert.Equal("event", Assert.Single(result.Value.Items).EventId);
        queries.VerifyAll();
        parser.VerifyAll();
    }

    [Fact]
    public async Task Endpoints_HaveUniqueMetadataAndSeparateResourcePermissions()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton(Mock.Of<IAuditTrailManager>());
        builder.Services.AddSingleton(Mock.Of<IAuditTrailAdminListFilterParser>());
        builder.Services.AddSingleton(Mock.Of<IAuditTrailAdminListQueryService>());
        builder.Services.AddSingleton(new BackgroundTaskManagementService([], new BackgroundTaskManager(Documents(new BackgroundTaskDocument()).Object, Mock.Of<ISignal>())));
        await using var app = builder.Build();
        ((IEndpointRouteBuilder)app).AddAuditTrailEndpoints();
        ((IEndpointRouteBuilder)app).AddBackgroundTaskEndpoints();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray();
        Assert.Equal(8, endpoints.Length);
        Assert.Equal(8, endpoints.Select(endpoint => endpoint.Metadata.GetRequiredMetadata<IEndpointNameMetadata>().EndpointName).Distinct().Count());
        foreach (var endpoint in endpoints)
        {
            var metadata = endpoint.Metadata.GetRequiredMetadata<CliOperationMetadata>();
            var policy = endpoint.Metadata.GetRequiredMetadata<AuthorizationPolicy>();
            Assert.Contains(OrchardCoreConstants.AuthenticationSchemes.Api, policy.AuthenticationSchemes);
            var permissions = policy.Requirements.OfType<global::OrchardCore.Security.PermissionRequirement>().Select(value => value.Permission.Name).ToArray();
            Assert.Contains("AccessRemoteManagement", permissions);
            Assert.Contains(metadata.Capability == "audit-trail" ? "ViewAuditTrail" : "ManageBackgroundTasks", permissions);
        }
    }

    [Theory]
    [InlineData("* * * * *", 0, 0, true)]
    [InlineData("*/5 0-23 * * 1-5", 100, 1000, true)]
    [InlineData("invalid", 0, 0, false)]
    [InlineData("* * * * * *", 0, 0, false)]
    [InlineData("* * * * *", -1, 0, false)]
    [InlineData("* * * * *", 0, -1, false)]
    [InlineData(null, 0, 0, false)]
    public async Task TaskAdminAndApi_ShareCronAndLockValidation(string schedule, int timeout, int expiration, bool valid)
    {
        var task = new FixtureTask();
        var document = new BackgroundTaskDocument();
        var documents = Documents(document);
        var manager = new BackgroundTaskManager(documents.Object, Mock.Of<ISignal>());
        var service = new BackgroundTaskManagementService([task], manager);
        var result = await service.UpdateAsync(task.GetTaskName(), new BackgroundTaskConfiguration
        {
            Schedule = schedule, LockTimeout = timeout, LockExpiration = expiration,
        });
        Assert.Equal(valid, result.Errors.Count == 0);
        var controller = Controller(task, manager);
        var adminResult = await controller.Edit(new BackgroundTaskViewModel
        {
            Name = task.GetTaskName(), Schedule = schedule, LockTimeout = timeout, LockExpiration = expiration,
        });
        Assert.Equal(valid, controller.ModelState.IsValid);
        if (valid) { Assert.IsType<RedirectToActionResult>(adminResult); }
        else
        {
            Assert.IsType<ViewResult>(adminResult);
            documents.Verify(value => value.UpdateAsync(It.IsAny<BackgroundTaskDocument>(), It.IsAny<Func<BackgroundTaskDocument, Task>>()), Times.Never);
            Assert.Empty(document.Settings);
        }
    }

    [Fact]
    public async Task TaskMutation_PreservesEnabledStateAndCachedSettingsAndSkipsRetries()
    {
        var task = new FixtureTask();
        var name = task.GetTaskName();
        var original = task.GetDefaultSettings();
        original.Enable = false;
        var document = new BackgroundTaskDocument();
        document.Settings[name] = original;
        var documents = Documents(document);
        var manager = new BackgroundTaskManager(documents.Object, Mock.Of<ISignal>());
        var service = new BackgroundTaskManagementService([task], manager);
        var configuration = new BackgroundTaskConfiguration { Schedule = "  */10 * * * *  ", Description = "New schedule", LockTimeout = 100, LockExpiration = 1000 };
        Assert.True((await service.UpdateAsync(name, configuration)).Changed);
        Assert.False(original.Enable);
        Assert.Equal("* * * * *", original.Schedule);
        var updated = await service.GetAsync(name);
        Assert.False(updated.Enable);
        Assert.Equal("*/10 * * * *", updated.Schedule);
        Assert.False((await service.UpdateAsync(name, configuration)).Changed);
        documents.Verify(value => value.UpdateAsync(document, It.IsAny<Func<BackgroundTaskDocument, Task>>()), Times.Once);
        await Controller(task, manager).Enable(name);
        Assert.True((await service.GetAsync(name)).Enable);
        Assert.False((await service.SetStatusAsync(name, true)).Changed);
        await Controller(task, manager).Disable(name);
        Assert.False((await service.GetAsync(name)).Enable);
        documents.Verify(value => value.UpdateAsync(document, It.IsAny<Func<BackgroundTaskDocument, Task>>()), Times.Exactly(3));
    }

    [Fact]
    public async Task TaskUpdateResponse_UsesWrittenSettingsBeforeDeferredCacheInvalidation()
    {
        var task = new FixtureTask();
        var name = task.GetTaskName();
        var immutable = new BackgroundTaskDocument();
        immutable.Settings[name] = task.GetDefaultSettings();
        var mutable = new BackgroundTaskDocument();
        var documents = Documents(mutable);
        documents.Setup(value => value.GetOrCreateImmutableAsync(It.IsAny<Func<Task<BackgroundTaskDocument>>>())).ReturnsAsync(immutable);
        var service = new BackgroundTaskManagementService([task], new BackgroundTaskManager(documents.Object, Mock.Of<ISignal>()));
        var response = Assert.IsType<Ok<BackgroundTaskResponse>>(await BackgroundTaskEndpoints.UpdateAsync(new DefaultHttpContext(), Authorization(), service,
            name, new BackgroundTaskConfiguration { Schedule = "*/15 * * * *", Description = "Updated" }));
        Assert.Equal("*/15 * * * *", response.Value.Configuration.Schedule);
        Assert.Equal("Updated", response.Value.Configuration.Description);
        Assert.Equal("* * * * *", immutable.Settings[name].Schedule);
        Assert.Equal("*/15 * * * *", mutable.Settings[name].Schedule);
    }

    [Fact]
    public async Task TaskWithInvalidPersistedSchedule_CanBeDisabledButCannotBeEnabled()
    {
        var task = new FixtureTask();
        var name = task.GetTaskName();
        var settings = task.GetDefaultSettings();
        settings.Schedule = "broken";
        var document = new BackgroundTaskDocument();
        document.Settings[name] = settings;
        var service = new BackgroundTaskManagementService([task], new BackgroundTaskManager(Documents(document).Object, Mock.Of<ISignal>()));
        Assert.True((await service.SetStatusAsync(name, false)).Changed);
        Assert.Contains("schedule", (await service.SetStatusAsync(name, true)).Errors.Keys);
        Assert.False((await service.GetAsync(name)).Enable);
        Assert.False((await service.UpdateAsync("missing", new BackgroundTaskConfiguration { Schedule = "* * * * *" })).Found);
        Assert.False((await service.SetStatusAsync("missing", false)).Found);
    }

    [Fact]
    public async Task AuditShow_UsesExistingManagerAndOmitsStoredPayloadAndIpAddress()
    {
        var manager = new Mock<IAuditTrailManager>();
        manager.Setup(value => value.GetEventAsync("event")).ReturnsAsync(new AuditTrailEvent
        {
            EventId = "event", Name = "Updated", Category = "Content", CorrelationId = "content",
            UserId = "actor", UserName = "admin", ClientIpAddress = "192.0.2.1", CreatedUtc = DateTime.UtcNow,
            Properties = new JsonObject { ["ContentSnapshot"] = new JsonObject { ["Secret"] = "must-not-leak" } },
        });
        var result = Assert.IsType<Ok<AuditTrailEventResponse>>(await AuditTrailEndpoints.GetAsync(new DefaultHttpContext(), Authorization(), manager.Object, "event"));
        Assert.Equal("content", result.Value.CorrelationId);
        var json = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain("must-not-leak", json, StringComparison.Ordinal);
        Assert.DoesNotContain("192.0.2.1", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Properties", json, StringComparison.Ordinal);
        manager.Verify(value => value.GetEventAsync("event"), Times.Once);
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 201)]
    [InlineData(int.MaxValue, 200)]
    public async Task AuditBounds_AreRejectedBeforeParsingOrQuerying(int page, int pageSize)
    {
        var parser = new Mock<IAuditTrailAdminListFilterParser>(MockBehavior.Strict);
        var queries = new Mock<IAuditTrailAdminListQueryService>(MockBehavior.Strict);
        Assert.Equal(400, Assert.IsType<ProblemHttpResult>(await AuditTrailEndpoints.ListAsync(new DefaultHttpContext(), Authorization(),
            parser.Object, queries.Object, null, page, pageSize)).StatusCode);
        parser.VerifyNoOtherCalls();
        queries.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeniedAuditAndTaskOperations_DoNotAccessServices()
    {
        using var services = new ServiceCollection().AddSingleton<IStringLocalizer<ProblemDetailsApiLocalization>>(
            new StringLocalizer<ProblemDetailsApiLocalization>(new NullStringLocalizerFactory())).BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        var authorization = Authorization(false);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await AuditTrailEndpoints.ListAsync(context, authorization, null, null, null, null, null)).StatusCode);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await AuditTrailEndpoints.GetAsync(context, authorization, null, "event")).StatusCode);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await BackgroundTaskEndpoints.UpdateAsync(context, authorization, null, "task", null)).StatusCode);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await BackgroundTaskEndpoints.EnableAsync(context, authorization, null, "task")).StatusCode);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await BackgroundTaskEndpoints.DisableAsync(context, authorization, null, "task")).StatusCode);
    }

    private static Mock<IDocumentManager<BackgroundTaskDocument>> Documents(BackgroundTaskDocument document)
    {
        var documents = new Mock<IDocumentManager<BackgroundTaskDocument>>();
        documents.Setup(value => value.GetOrCreateImmutableAsync(It.IsAny<Func<Task<BackgroundTaskDocument>>>())).ReturnsAsync(document);
        documents.Setup(value => value.GetOrCreateMutableAsync(It.IsAny<Func<Task<BackgroundTaskDocument>>>())).ReturnsAsync(document);
        return documents;
    }

    private static BackgroundTaskController Controller(IBackgroundTask task, BackgroundTaskManager manager) =>
        new(Authorization(), [task], manager, Options.Create(new PagerOptions()), Mock.Of<IShapeFactory>(),
            Mock.Of<IHtmlLocalizer<BackgroundTaskController>>(), new StringLocalizer<BackgroundTaskController>(new NullStringLocalizerFactory()), Mock.Of<INotifier>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

    private static IAuthorizationService Authorization(bool allowed = true)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }

    private sealed class FixtureTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
