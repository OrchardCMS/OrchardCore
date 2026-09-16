using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Controllers;
using OrchardCore.Deployment.Steps;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentStepManagementTests
{
    [Fact]
    public async Task SharedSteps_ValidateBatchesAndOrdersBeforeMutation()
    {
        using var site = await CreateContextAsync();
        long id = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = (await plans.CreateAsync("Steps")).Plan;
            id = plan.Id;
            var first = new CustomFileDeploymentStep { Id = "first", FileName = "one.txt", FileContent = "original" };
            var second = new RecipeFileDeploymentStep { Id = "second" };
            Assert.True((await plans.AddStepsAsync(id, [first, second])).Changed);
            Assert.False((await plans.AddStepsAsync(id, [])).Changed);
            Assert.Equal(DeploymentStepManagementError.InvalidStep, (await plans.AddStepsAsync(id,
                [new RecipeFileDeploymentStep { Id = "third" }, new RecipeFileDeploymentStep { Id = "FIRST" }])).Error);
            var repeated = new RecipeFileDeploymentStep();
            Assert.Equal(DeploymentStepManagementError.InvalidStep, (await plans.AddStepsAsync(id, [repeated, repeated])).Error);
            Assert.Equal(new[] { "first", "second" }, plan.DeploymentSteps.Select(step => step.Id));

            Assert.Equal(DeploymentStepManagementError.InvalidOrder, (await plans.ReorderStepsAsync(id, ["second", "missing"])).Error);
            Assert.Equal(DeploymentStepManagementError.InvalidOrder, (await plans.ReorderStepsAsync(id, ["first", "FIRST"])).Error);
            Assert.Equal(new[] { "first", "second" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.True((await plans.ReorderStepsAsync(id, ["second", "first"])).Changed);
            Assert.False((await plans.ReorderStepsAsync(id, ["second", "first"])).Changed);
            Assert.False((await plans.MoveStepAsync(id, 0, 0)).Changed);

            var candidate = Assert.IsType<CustomFileDeploymentStep>(plans.CloneStep(first));
            Assert.False((await plans.UpdateStepAsync(id, candidate)).Changed);
            candidate.FileContent = "changed";
            Assert.Equal("original", first.FileContent);
            Assert.True((await plans.UpdateStepAsync(id, candidate)).Changed);
            Assert.Equal(DeploymentStepManagementError.InvalidStep,
                (await plans.UpdateStepAsync(id, new RecipeFileDeploymentStep { Id = "first" })).Error);
            Assert.Equal("changed", Assert.IsType<CustomFileDeploymentStep>(plan.DeploymentSteps[1]).FileContent);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = await plans.GetAsync(id);
            Assert.Equal(new[] { "second", "first" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.Equal("changed", Assert.IsType<CustomFileDeploymentStep>(plan.DeploymentSteps[1]).FileContent);
            Assert.True((await plans.DeleteStepAsync(id, "second")).Changed);
            Assert.Equal(DeploymentStepManagementError.NotFound, (await plans.DeleteStepAsync(id, "second")).Error);
            Assert.Equal("first", Assert.Single(plan.DeploymentSteps).Id);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AdminEdit_OnlyAppliesValidDetachedCandidate(bool valid)
    {
        using var site = await CreateContextAsync();
        long id = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            id = (await plans.CreateAsync("Editor")).Plan.Id;
            await plans.AddStepsAsync(id, [new CustomFileDeploymentStep { Id = "file", FileName = "one.txt", FileContent = "original" }]);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var original = Assert.IsType<CustomFileDeploymentStep>(Assert.Single((await plans.GetAsync(id)).DeploymentSteps));
            var display = new Mock<IDisplayManager<DeploymentStep>>();
            var accessor = new Mock<IUpdateModelAccessor>();
            accessor.SetupGet(value => value.ModelUpdater).Returns(Mock.Of<IUpdateModel>());
            var controller = ActivatorUtilities.CreateInstance<StepController>(scope.ServiceProvider,
                display.Object, accessor.Object, Authorized(), Mock.Of<INotifier>());
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider } };
            controller.Url = Mock.Of<IUrlHelper>();
            controller.TempData = Mock.Of<global::Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
            display.Setup(manager => manager.UpdateEditorAsync(It.IsAny<DeploymentStep>(), It.IsAny<IUpdateModel>(), false, "", ""))
                .Callback((DeploymentStep step, IUpdateModel _, bool _, string _, string _) =>
                {
                    Assert.NotSame(original, step);
                    Assert.IsType<CustomFileDeploymentStep>(step).FileContent = "edited";
                    if (!valid) { controller.ModelState.AddModelError("FileName", "Rejected editor value"); }
                }).ReturnsAsync(new Shape());
            var result = await controller.Edit(new EditDeploymentPlanStepViewModel { DeploymentPlanId = id, DeploymentStepId = "file" });
            if (valid) { Assert.IsType<RedirectToActionResult>(result); }
            else { Assert.IsType<ViewResult>(result); }
            Assert.Equal("original", original.FileContent);
            Assert.Equal(valid ? "edited" : "original", Assert.IsType<CustomFileDeploymentStep>(Assert.Single((await plans.GetAsync(id)).DeploymentSteps)).FileContent);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            Assert.Equal(valid ? "edited" : "original", Assert.IsType<CustomFileDeploymentStep>(Assert.Single((await plans.GetAsync(id)).DeploymentSteps)).FileContent);
        });
    }

    private static IAuthorizationService Authorized()
    {
        var auth = new Mock<IAuthorizationService>();
        auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        return auth.Object;
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await features.EnableFeaturesAsync([(await features.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Deployment")], force: true);
        });
        return site;
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task AdminReorder_InvalidDestination_PreservesTrackedSteps(int destination)
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await features.EnableFeaturesAsync([(await features.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Deployment")], force: true);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = new DeploymentPlan
            {
                Name = "Order test",
                DeploymentSteps = [new RecipeFileDeploymentStep { Id = "first" }, new RecipeFileDeploymentStep { Id = "second" }],
            };
            await plans.CreateOrUpdateDeploymentPlansAsync([plan]);
            var auth = new Mock<IAuthorizationService>();
            auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var controller = new StepController(auth.Object, null, [], plans, null, null, null)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            };
            IActionResult result = null;
            var error = await Record.ExceptionAsync(async () => result = await controller.UpdateOrder(plan.Id, 0, destination));
            Assert.Equal(new[] { "first", "second" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.Null(error);
            Assert.IsType<BadRequestResult>(result);
        });
    }
}
