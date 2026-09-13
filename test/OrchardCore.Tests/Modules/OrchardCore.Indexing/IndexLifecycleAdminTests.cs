using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Controllers;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Operations;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Tests.Apis.Context;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexLifecycleAdminTests
{
    [Theory]
    [InlineData(IndexLifecycleAction.Reset, false)]
    [InlineData(IndexLifecycleAction.Rebuild, false)]
    [InlineData(IndexLifecycleAction.Synchronize, false)]
    [InlineData(IndexLifecycleAction.Reset, true)]
    [InlineData(IndexLifecycleAction.Rebuild, true)]
    [InlineData(IndexLifecycleAction.Synchronize, true)]
    public async Task Action_QueuesOnceAndSkipsMissingBulkProfiles(IndexLifecycleAction action, bool bulk)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        string operationId = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
            profiles.Setup(value => value.FindByIdAsync("index")).ReturnsAsync(new IndexProfile { Id = "index" });
            profiles.Setup(value => value.FindByIdAsync("missing")).ReturnsAsync((IndexProfile)null);
            var controller = Controller(profiles.Object, scope.ServiceProvider.GetRequiredService<IndexOperationRunner>());

            var result = bulk
                ? await controller.IndexPost(new IndexingEntityOptions { BulkAction = Enum.Parse<IndexingEntityAction>(action.ToString()) }, ["index", "index", "missing"])
                : await Invoke(controller, action);

            Assert.IsType<RedirectToActionResult>(result);
            using var session = scope.ServiceProvider.GetRequiredService<IStore>().CreateSession();
            var operation = Assert.Single(await session.Query<IndexOperation>().ListAsync());
            operationId = operation.OperationId;
            Assert.Equal("index", operation.IndexId);
            Assert.Equal(action, operation.Action);
            Assert.Equal(IndexOperationState.Pending, operation.State);
            profiles.Verify(value => value.FindByIdAsync("index"), Times.Once());
            if (bulk) { profiles.Verify(value => value.FindByIdAsync("missing"), Times.Once()); }
            profiles.VerifyNoOtherCalls();
        });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        var finished = false;
        while (!finished)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var operation = await scope.ServiceProvider.GetRequiredService<IndexOperationStore>().FindAsync(operationId);
                if (operation.State is IndexOperationState.Pending or IndexOperationState.Running) { return; }
                Assert.Equal(IndexOperationState.Failed, operation.State);
                Assert.Equal(IndexProcessingStatus.NotFound, operation.Outcome);
                finished = true;
            });
            Assert.True(finished || DateTime.UtcNow < deadline, "Admin background operation did not finish.");
            if (!finished) { await Task.Delay(100, TestContext.Current.CancellationToken); }
        }
    }

    [Theory]
    [InlineData(IndexLifecycleAction.Reset)]
    [InlineData(IndexLifecycleAction.Rebuild)]
    [InlineData(IndexLifecycleAction.Synchronize)]
    public async Task Action_DeniedOrMissing_DoesNotQueue(IndexLifecycleAction action)
    {
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        var denied = Controller(profiles.Object, null, authorized: false);
        Assert.IsType<ForbidResult>(await Invoke(denied, action));
        Assert.IsType<ForbidResult>(await denied.IndexPost(new IndexingEntityOptions
            { BulkAction = Enum.Parse<IndexingEntityAction>(action.ToString()) }, ["index"]));
        profiles.VerifyNoOtherCalls();

        profiles.Setup(value => value.FindByIdAsync("index")).ReturnsAsync((IndexProfile)null);
        Assert.IsType<NotFoundResult>(await Invoke(Controller(profiles.Object, null), action));
    }

    private static Task<IActionResult> Invoke(AdminController controller, IndexLifecycleAction action) => action switch
    {
        IndexLifecycleAction.Reset => controller.Reset("index"),
        IndexLifecycleAction.Rebuild => controller.Rebuild("index"),
        _ => controller.Synchronize("index"),
    };

    private static AdminController Controller(IIndexProfileManager profiles, IndexOperationRunner runner, bool authorized = true)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return new AdminController(authorization.Object, null, profiles, null, runner, null, Options.Create(new IndexingOptions()),
            Mock.Of<INotifier>(), Mock.Of<IHtmlLocalizer<AdminController>>(), Mock.Of<IStringLocalizer<AdminController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }
}
