using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Deployment.AddToDeploymentPlan;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentContentControllerTests
{
    [Fact]
    public async Task DisabledContentFeature_PreservesStepAcrossPlanEdit_AndRestoresTypedConfiguration()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await manager.GetAvailableFeaturesAsync();
            await manager.EnableFeaturesAsync(available.Where(feature => feature.Id is
                "OrchardCore.Deployment" or "OrchardCore.Contents.Deployment.AddToDeploymentPlan"), force: true);
        });
        long planId = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            planId = (await plans.CreateAsync("Feature lifecycle")).Plan.Id;
            Assert.True((await plans.AddStepsAsync(planId,
                [new ContentItemDeploymentStep { Id = "keep-id", ContentItemId = "keep-reference" }])).Changed);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await manager.DisableFeaturesAsync((await manager.GetAvailableFeaturesAsync()).Where(feature =>
                feature.Id == "OrchardCore.Contents.Deployment.AddToDeploymentPlan"), force: true);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var registry = scope.ServiceProvider.GetRequiredService<DeploymentStepRegistry>();
            Assert.DoesNotContain(registry.List(), type => type.Type == nameof(ContentItemDeploymentStep));
            var plan = await plans.GetAsync(planId);
            var unknown = Assert.IsType<UnknownDeploymentStep>(Assert.Single(plan.DeploymentSteps));
            Assert.Equal("keep-id", unknown.Id);
            Assert.Null(registry.Definition(unknown));
            Assert.True((await plans.RenameAsync(planId, "Feature disabled")).Changed);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await manager.EnableFeaturesAsync((await manager.GetAvailableFeaturesAsync()).Where(feature =>
                feature.Id == "OrchardCore.Contents.Deployment.AddToDeploymentPlan"), force: true);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plan = await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetAsync(planId);
            Assert.Equal("Feature disabled", plan.Name);
            var step = Assert.IsType<ContentItemDeploymentStep>(Assert.Single(plan.DeploymentSteps));
            Assert.Equal("keep-id", step.Id);
            Assert.Equal("keep-reference", step.ContentItemId);
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<DeploymentStepRegistry>().Definition(step));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingContentActions_PersistUniqueIds_AndRejectBulkBeforeMutation(bool denySecondItem)
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await manager.GetAvailableFeaturesAsync();
            await manager.EnableFeaturesAsync(available.Where(feature => feature.Id is
                "OrchardCore.Deployment" or "OrchardCore.Contents.Deployment.AddToDeploymentPlan"), force: true);
        });
        long planId = 0;
        var items = new List<ContentItem>();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentManager>();
            for (var index = 0; index < 2; index++)
            {
                var item = await content.NewAsync("BlogPost");
                item.DisplayText = $"Deployment content {index}";
                Assert.True(await content.CreateAsync(item, VersionOptions.Published));
                items.Add(item);
            }
            planId = (await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().CreateAsync("Content selection")).Plan.Id;
        });
        string firstId = null;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var contentChecks = 0;
            var reject = false;
            var auth = new Mock<IAuthorizationService>();
            auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync((ClaimsPrincipal _, object resource, IEnumerable<IAuthorizationRequirement> _) =>
                    resource is ContentItem && ++contentChecks == 2 && reject
                        ? AuthorizationResult.Failed() : AuthorizationResult.Success());
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var controller = new AddToDeploymentPlanController(auth.Object,
                scope.ServiceProvider.GetRequiredService<IContentManager>(),
                scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>(), plans,
                scope.ServiceProvider.GetServices<IDeploymentStepFactory>(), Mock.Of<INotifier>(),
                Mock.Of<IHtmlLocalizer<AddToDeploymentPlanController>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider } },
                Url = Mock.Of<IUrlHelper>(),
            };
            Assert.IsType<LocalRedirectResult>(await controller.AddContentItem(planId, "/", items[0].ContentItemId));
            var plan = await plans.GetAsync(planId);
            firstId = Assert.Single(plan.DeploymentSteps).Id;
            Assert.False(string.IsNullOrWhiteSpace(firstId));
            contentChecks = 0;
            reject = denySecondItem;
            var result = await controller.AddContentItems(planId, "/", items.Select(item => item.Id));
            Assert.Equal(2, contentChecks);
            if (denySecondItem)
            {
                Assert.IsType<ForbidResult>(result);
                Assert.Equal(firstId, Assert.Single(plan.DeploymentSteps).Id);
            }
            else
            {
                Assert.IsType<LocalRedirectResult>(result);
                Assert.Equal(3, plan.DeploymentSteps.Count);
            }
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plan = await scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>().GetAsync(planId);
            Assert.Equal(denySecondItem ? 1 : 3, plan.DeploymentSteps.Count);
            Assert.Equal(firstId, plan.DeploymentSteps[0].Id);
            Assert.Equal(plan.DeploymentSteps.Count, plan.DeploymentSteps.Select(step => step.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.All(plan.DeploymentSteps, step =>
            {
                Assert.False(string.IsNullOrWhiteSpace(step.Id));
                Assert.Contains(Assert.IsType<ContentItemDeploymentStep>(step).ContentItemId, items.Select(item => item.ContentItemId));
            });
        });
    }
}
