using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Controllers;
using OrchardCore.Deployment.Endpoints.Management;
using OrchardCore.Deployment.Steps;
using OrchardCore.Deployment.Services;
using System.Text.Json.Nodes;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using OrchardCore.Localization;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentPlanManagementTests : IDisposable
{
    private readonly ServiceProvider _services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task RemoteSteps_PersistTypedEdits_WithoutLeakingOrPartiallyMutatingConfiguration()
    {
        using var site = await CreateContextAsync();
        long id = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var registry = scope.ServiceProvider.GetRequiredService<DeploymentStepRegistry>();
            id = (await plans.CreateAsync("Remote steps")).Plan.Id;
            var request = new DeploymentStepCreateRequest
            {
                Id = "file", Type = nameof(CustomFileDeploymentStep),
                Values = new() { ["fileName"] = "one.txt", ["fileContent"] = "private-content" },
            };
            var added = StepWrite(await DeploymentStepEndpoints.AddAsync(Http(), Authorized(), plans, registry, id, request));
            Assert.True(added.Changed);
            Assert.DoesNotContain("private-content", JsonSerializer.Serialize(added));
            Assert.False(StepWrite(await DeploymentStepEndpoints.AddAsync(Http(), Authorized(), plans, registry, id, request)).Changed);
            Assert.Equal(409, Status(await DeploymentStepEndpoints.AddAsync(Http(), Authorized(), plans, registry, id,
                new() { Id = "file", Type = nameof(CustomFileDeploymentStep), Values = new() { ["fileContent"] = "conflict" } })));
            var plan = await plans.GetAsync(id);
            Assert.Equal("private-content", Assert.IsType<CustomFileDeploymentStep>(Assert.Single(plan.DeploymentSteps)).FileContent);
            Assert.Equal(400, Status(await DeploymentStepEndpoints.UpdateAsync(Http(), Authorized(), plans, registry, id, "file",
                new() { Values = new() { ["fileName"] = "../invalid", ["fileContent"] = "rejected" } })));
            Assert.Equal("one.txt", Assert.IsType<CustomFileDeploymentStep>(Assert.Single(plan.DeploymentSteps)).FileName);
            Assert.Equal("private-content", Assert.IsType<CustomFileDeploymentStep>(Assert.Single(plan.DeploymentSteps)).FileContent);
            Assert.True(StepWrite(await DeploymentStepEndpoints.UpdateAsync(Http(), Authorized(), plans, registry, id, "file",
                new() { Values = new() { ["fileName"] = "two.txt" } })).Changed);
            Assert.Equal("private-content", Assert.IsType<CustomFileDeploymentStep>(Assert.Single(plan.DeploymentSteps)).FileContent);
            Assert.True(StepWrite(await DeploymentStepEndpoints.UpdateAsync(Http(), Authorized(), plans, registry, id, "file",
                new() { Values = new() { ["fileContent"] = null } })).Changed);
            Assert.False(StepWrite(await DeploymentStepEndpoints.UpdateAsync(Http(), Authorized(), plans, registry, id, "file",
                new() { Values = new() { ["fileContent"] = null } })).Changed);
            Assert.True(StepWrite(await DeploymentStepEndpoints.AddAsync(Http(), Authorized(), plans, registry, id,
                new() { Id = "recipe", Type = nameof(RecipeFileDeploymentStep), Values = new() { ["recipeName"] = "Export" } })).Changed);
            Assert.Equal(400, Status(await DeploymentStepEndpoints.OrderAsync(Http(), Authorized(), plans, id, new() { StepIds = ["file", "file"] })));
            Assert.Equal(new[] { "file", "recipe" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.True(StepWrite(await DeploymentStepEndpoints.OrderAsync(Http(), Authorized(), plans, id, new() { StepIds = ["recipe", "file"] })).Changed);
            Assert.False(StepWrite(await DeploymentStepEndpoints.OrderAsync(Http(), Authorized(), plans, id, new() { StepIds = ["recipe", "file"] })).Changed);
            var listed = Assert.IsType<Ok<IReadOnlyList<DeploymentStepResponse>>>(await DeploymentStepEndpoints.ListAsync(Http(), Authorized(), plans, registry, id)).Value;
            Assert.Equal(new[] { "recipe", "file" }, listed.Select(step => step.Id));
            var shown = Assert.IsType<Ok<DeploymentStepResponse>>(await DeploymentStepEndpoints.GetAsync(Http(), Authorized(), plans, registry, id, "file")).Value;
            Assert.Equal(1, shown.Position);
            Assert.Equal("two.txt", shown.Values["fileName"].GetValue<string>());
            Assert.False(shown.Values.ContainsKey("fileContent"));
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = await plans.GetAsync(id);
            Assert.Equal(new[] { "recipe", "file" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.Equal(string.Empty, Assert.IsType<CustomFileDeploymentStep>(plan.DeploymentSteps[1]).FileContent);
            Assert.True(StepWrite(await DeploymentStepEndpoints.DeleteAsync(Http(), Authorized(), plans, id, "file")).Changed);
            Assert.False(StepWrite(await DeploymentStepEndpoints.DeleteAsync(Http(), Authorized(), plans, id, "file")).Changed);
            Assert.Equal("recipe", Assert.Single(plan.DeploymentSteps).Id);
        });
    }

    [Fact]
    public async Task RemoteSteps_DenyBeforeStoreAccess()
    {
        var plans = new Mock<IDeploymentPlanService>(MockBehavior.Strict);
        var registry = new DeploymentStepRegistry([], []);
        var denied = Authorize(DeploymentPermissions.ManageDeploymentPlan);
        Assert.Equal(403, Status(await DeploymentStepEndpoints.ListAsync(Http(), denied, plans.Object, registry, 1)));
        Assert.Equal(403, Status(await DeploymentStepEndpoints.GetAsync(Http(), denied, plans.Object, registry, 1, "step")));
        Assert.Equal(403, Status(await DeploymentStepEndpoints.AddAsync(Http(), denied, plans.Object, registry, 1, new())));
        Assert.Equal(403, Status(await DeploymentStepEndpoints.UpdateAsync(Http(), denied, plans.Object, registry, 1, "step", new())));
        Assert.Equal(403, Status(await DeploymentStepEndpoints.DeleteAsync(Http(), denied, plans.Object, 1, "step")));
        Assert.Equal(403, Status(await DeploymentStepEndpoints.OrderAsync(Http(), denied, plans.Object, 1, new())));
        plans.VerifyNoOtherCalls();
    }

    private static DeploymentStepWriteResponse StepWrite(IResult result) => Assert.IsType<Ok<DeploymentStepWriteResponse>>(result).Value;

    [Fact]
    public async Task StepSchemas_DistinguishUnsupportedAndUnavailableFactories()
    {
        var known = new Mock<IDeploymentStepFactory>();
        known.SetupGet(factory => factory.Name).Returns(nameof(CustomFileDeploymentStep));
        var extension = new Mock<IDeploymentStepFactory>();
        extension.SetupGet(factory => factory.Name).Returns("ExtensionStep");
        var registry = new DeploymentStepRegistry([known.Object, extension.Object],
            [new BuiltInDeploymentStepDefinition(nameof(CustomFileDeploymentStep))]);
        var list = Assert.IsType<Ok<IReadOnlyList<DeploymentStepTypeDescriptor>>>(await DeploymentStepTypeEndpoints.ListAsync(Http(), Authorized(), registry)).Value;
        Assert.True(Assert.Single(list, item => item.Type == nameof(CustomFileDeploymentStep)).CanConfigure);
        Assert.False(Assert.Single(list, item => item.Type == "ExtensionStep").CanConfigure);
        var schema = Assert.IsType<Ok<JsonObject>>(await DeploymentStepTypeEndpoints.SchemaAsync(Http(), Authorized(), registry, nameof(CustomFileDeploymentStep))).Value;
        Assert.True(schema["properties"]["fileContent"]["writeOnly"].GetValue<bool>());
        Assert.Equal(501, Status(await DeploymentStepTypeEndpoints.SchemaAsync(Http(), Authorized(), registry, "ExtensionStep")));
        Assert.Equal(404, Status(await DeploymentStepTypeEndpoints.SchemaAsync(Http(), Authorized(), registry, "DisabledStep")));
        Assert.Equal(400, Status(await DeploymentStepTypeEndpoints.SchemaAsync(Http(), Authorized(), registry, "")));
        Assert.Equal(403, Status(await DeploymentStepTypeEndpoints.ListAsync(Http(), Authorize(DeploymentPermissions.ManageDeploymentPlan), registry)));
        Assert.Equal(403, Status(await DeploymentStepTypeEndpoints.SchemaAsync(Http(), Authorize(RemoteManagementPermissions.AccessRemoteManagement), registry, nameof(CustomFileDeploymentStep))));
        known.Verify(factory => factory.Create(), Times.Never);
        extension.Verify(factory => factory.Create(), Times.Never);
    }

    [Fact]
    public async Task TenantRegistry_ContainsBuiltInContracts_AndPreservesGenericFactoryIdentity()
    {
        using var site = await CreateContextAsync();
        await site.UsingTenantScopeAsync(scope =>
        {
            var registry = scope.ServiceProvider.GetRequiredService<DeploymentStepRegistry>();
            Assert.True(Assert.Single(registry.List(), item => item.Type == nameof(CustomFileDeploymentStep)).CanConfigure);
            Assert.True(Assert.Single(registry.List(), item => item.Type == nameof(JsonRecipeDeploymentStep)).CanConfigure);
            var factory = new global::OrchardCore.Settings.Deployment.SiteSettingsPropertyDeploymentStepFactory<global::OrchardCore.Search.Models.SearchSettings>();
            var step = factory.Create();
            Assert.Equal(factory.Name, DeploymentStepTypeResolver.Resolve(step, new Dictionary<string, IDeploymentStepFactory> { [factory.Name] = factory }));
            Assert.Equal(nameof(JsonRecipeDeploymentStep), DeploymentStepTypeResolver.Resolve(new JsonRecipeDeploymentStep(), new Dictionary<string, IDeploymentStepFactory>()));
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task AdminAndApi_EditSamePlan_PreserveStepsAndRejectConflicts()
    {
        using var site = await CreateContextAsync();
        long id = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var admin = Admin(plans);
            Assert.IsType<RedirectToActionResult>(await admin.Create(new CreateDeploymentPlanViewModel { Name = "Alpha" }));
            var plan = await plans.FindByNameAsync("Alpha");
            id = plan.Id;
            plan.DeploymentSteps.Add(new CustomFileDeploymentStep { Id = "file", FileName = "example.txt", FileContent = "private-step-content" });
            plan.DeploymentSteps.Add(new RecipeFileDeploymentStep { Id = "recipe" });
            await plans.CreateOrUpdateDeploymentPlansAsync([plan]);

            var retry = Write(await DeploymentPlanEndpoints.CreateAsync(Http(), Authorized(), plans, new() { Name = "Alpha" }));
            Assert.False(retry.Changed);
            Assert.Equal(id, retry.Plan.Id);
            Assert.Equal(2, retry.Plan.StepCount);
            Assert.DoesNotContain("private-step-content", JsonSerializer.Serialize(retry));

            Assert.True(Write(await DeploymentPlanEndpoints.UpdateAsync(Http(), Authorized(), plans, id, new() { Name = "Beta" })).Changed);
            Assert.False(Write(await DeploymentPlanEndpoints.UpdateAsync(Http(), Authorized(), plans, id, new() { Name = "Beta" })).Changed);
            Assert.IsType<RedirectToActionResult>(await Admin(plans).Edit(new EditDeploymentPlanViewModel { Id = id, Name = "Gamma" }));
            Assert.Equal(new[] { "file", "recipe" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.Equal("private-step-content", Assert.IsType<CustomFileDeploymentStep>(plan.DeploymentSteps[0]).FileContent);

            await plans.CreateAsync("Taken");
            Assert.Equal(409, Status(await DeploymentPlanEndpoints.UpdateAsync(Http(), Authorized(), plans, id, new() { Name = "Taken" })));
            Assert.Equal("Gamma", plan.Name);
            var rejected = Admin(plans);
            Assert.IsType<ViewResult>(await rejected.Edit(new EditDeploymentPlanViewModel { Id = id, Name = "Taken" }));
            Assert.False(rejected.ModelState.IsValid);
            Assert.Equal("Gamma", plan.Name);
            Assert.Equal(new[] { "file", "recipe" }, plan.DeploymentSteps.Select(step => step.Id));

            var page = Assert.IsType<Ok<DeploymentPlanListResponse>>(await DeploymentPlanEndpoints.ListAsync(Http(), Authorized(), plans,
                new() { Search = "Gamma", Take = 1 })).Value;
            Assert.Equal(1, page.TotalCount);
            Assert.Equal(id, Assert.Single(page.Items).Id);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var plan = await plans.GetAsync(id);
            Assert.Equal("Gamma", plan.Name);
            Assert.Equal(new[] { "file", "recipe" }, plan.DeploymentSteps.Select(step => step.Id));
            Assert.True(Write(await DeploymentPlanEndpoints.DeleteAsync(Http(), Authorized(), plans, id)).Changed);
            Assert.False(Write(await DeploymentPlanEndpoints.DeleteAsync(Http(), Authorized(), plans, id)).Changed);
            Assert.Equal(404, Status(await DeploymentPlanEndpoints.GetAsync(Http(), Authorized(), plans, id)));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Endpoints_RequireBothPermissions_BeforeAccessingPlans(bool remoteOnly)
    {
        var auth = Authorize(remoteOnly ? RemoteManagementPermissions.AccessRemoteManagement : DeploymentPermissions.ManageDeploymentPlan);
        var plans = new Mock<IDeploymentPlanService>(MockBehavior.Strict);
        Assert.Equal(403, Status(await DeploymentPlanEndpoints.ListAsync(Http(), auth, plans.Object, new())));
        Assert.Equal(403, Status(await DeploymentPlanEndpoints.GetAsync(Http(), auth, plans.Object, 1)));
        Assert.Equal(403, Status(await DeploymentPlanEndpoints.CreateAsync(Http(), auth, plans.Object, new() { Name = "New" })));
        Assert.Equal(403, Status(await DeploymentPlanEndpoints.UpdateAsync(Http(), auth, plans.Object, 1, new() { Name = "New" })));
        Assert.Equal(403, Status(await DeploymentPlanEndpoints.DeleteAsync(Http(), auth, plans.Object, 1)));
        plans.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidRequests_DoNotReachPlanStore()
    {
        var plans = new Mock<IDeploymentPlanService>(MockBehavior.Strict);
        Assert.Equal(400, Status(await DeploymentPlanEndpoints.ListAsync(Http(), Authorized(), plans.Object, new() { Take = 201 })));
        Assert.Equal(400, Status(await DeploymentPlanEndpoints.ListAsync(Http(), Authorized(), plans.Object, new() { Skip = -1 })));
        Assert.Equal(400, Status(await DeploymentPlanEndpoints.GetAsync(Http(), Authorized(), plans.Object, 0)));
        Assert.Equal(400, Status(await DeploymentPlanEndpoints.CreateAsync(Http(), Authorized(), plans.Object, new() { Name = " " })));
        Assert.Equal(400, Status(await DeploymentPlanEndpoints.UpdateAsync(Http(), Authorized(), plans.Object, 1, new() { Name = " " })));
        Assert.Equal(400, Status(await DeploymentPlanEndpoints.DeleteAsync(Http(), Authorized(), plans.Object, -1)));
        plans.VerifyNoOtherCalls();
    }

    private DeploymentPlanController Admin(IDeploymentPlanService plans) => new(
        Authorize(DeploymentPermissions.ManageDeploymentPlan), null, [], plans, Options.Create(new PagerOptions()), null,
        new StringLocalizer<DeploymentPlanController>(new NullStringLocalizerFactory()), Mock.Of<IHtmlLocalizer<DeploymentPlanController>>(),
        Mock.Of<INotifier>(), null)
    {
        ControllerContext = new ControllerContext { HttpContext = Http() },
        Url = Mock.Of<IUrlHelper>(),
        TempData = Mock.Of<ITempDataDictionary>(),
    };

    private DefaultHttpContext Http() => new() { RequestServices = _services };
    private static DeploymentPlanWriteResponse Write(IResult result) => Assert.IsType<Ok<DeploymentPlanWriteResponse>>(result).Value;
    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;
    private static IAuthorizationService Authorized() => Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.ManageDeploymentPlan);
    private static IAuthorizationService Authorize(params Permission[] permissions)
    {
        var auth = new Mock<IAuthorizationService>();
        auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => permissions.Any(permission => permission.Name == requirement.Permission.Name))
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return auth.Object;
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Deployment");
            await manager.EnableFeaturesAsync([feature], force: true);
        });
        return context;
    }
}
