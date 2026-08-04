using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.RateLimits.Controllers;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.ViewModels;
using OrchardCore.RateLimits;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class AdminControllerTests
{
    [Fact]
    public async Task EditPost_Default_SavessEnabledPolicyMetadataWithoutReloadingShell()
    {
        const string policyId = "policy-id";
        var enabledUtc = DateTime.UtcNow;
        var existingPolicy = new RateLimitPolicy
        {
            PolicyId = policyId,
            Name = "Original policy",
            Description = "Original description",
            Scope = RateLimitPolicyScope.Global,
            IsEnabled = true,
            EnabledUtc = enabledUtc,
            Limiters =
            [
                new RateLimitLimiter
                {
                    Id = "limiter-id",
                    Source = "FixedWindow",
                },
            ],
        };

        var policyStore = new Mock<IRateLimitPolicyStore>();
        policyStore
            .Setup(x => x.FindByIdAsync(policyId, PolicyVersion.Current))
            .Returns(() => ValueTask.FromResult(existingPolicy));
        policyStore
            .Setup(x => x.GetAllAsync(PolicyVersion.Current))
            .Returns(() => ValueTask.FromResult<IReadOnlyCollection<RateLimitPolicy>>([existingPolicy]));

        RateLimitPolicy savedPolicy = null;
        policyStore
            .Setup(x => x.UpdateAsync(It.IsAny<RateLimitPolicy>()))
            .Callback<RateLimitPolicy>(policy => savedPolicy = policy)
            .Returns(ValueTask.CompletedTask);

        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var controller = CreateController(policyStore.Object, shellReleaseManager.Object);

        var model = new RateLimitPolicyEditViewModel
        {
            Name = "Renamed policy",
            Description = "Updated description",
            Scope = RateLimitPolicyScope.Endpoint,
            Path = "invalid-path",
            GroupName = "tampered-group",
            IsEnabled = true,
        };

        var result = await controller.EditPost(policyId, model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Index), redirectResult.ActionName);
        Assert.NotNull(savedPolicy);
        Assert.Equal("Renamed policy", savedPolicy.Name);
        Assert.Equal("Updated description", savedPolicy.Description);
        Assert.Equal(existingPolicy.Scope, savedPolicy.Scope);
        Assert.Equal(existingPolicy.Path, savedPolicy.Path);
        Assert.Equal(existingPolicy.GroupName, savedPolicy.GroupName);
        Assert.True(savedPolicy.IsEnabled);
        Assert.Equal(enabledUtc, savedPolicy.EnabledUtc);
        Assert.Single(savedPolicy.Limiters);
        Assert.Equal("limiter-id", savedPolicy.Limiters[0].Id);
        shellReleaseManager.Verify(x => x.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task Enable_EnablingDisabledPolicy_ReloadShell()
    {
        const string policyId = "policy-id";
        var existingPolicy = new RateLimitPolicy
        {
            PolicyId = policyId,
            Name = "Disabled policy",
            Scope = RateLimitPolicyScope.Global,
            IsEnabled = false,
        };

        var policyStore = new Mock<IRateLimitPolicyStore>();
        policyStore
            .Setup(x => x.FindByIdAsync(policyId, PolicyVersion.Current))
            .Returns(() => ValueTask.FromResult(existingPolicy));
        policyStore
            .Setup(x => x.SetStatusAsync(policyId, true))
            .Returns(() => ValueTask.FromResult(true));

        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var controller = CreateController(policyStore.Object, shellReleaseManager.Object);

        var result = await controller.Enable(policyId);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Index), redirectResult.ActionName);
        shellReleaseManager.Verify(x => x.RequestRelease(), Times.Once);
    }

    [Fact]
    public async Task Clone_Default_CreatesDisabledPolicyWithUniqueIncrementedName()
    {
        const string policyId = "policy-id";
        var sourcePolicy = new RateLimitPolicy
        {
            PolicyId = policyId,
            Name = "Login policy (1)",
            Description = "Original description",
            Scope = RateLimitPolicyScope.Endpoint,
            Path = "/login",
            GroupName = "users",
            IsEnabled = true,
            EnabledUtc = DateTime.UtcNow,
            Limiters =
            [
                new RateLimitLimiter
                {
                    Id = "limiter-id",
                    Source = "FixedWindow",
                    Properties =
                    {
                        ["PermitLimit"] = 10,
                    },
                },
            ],
        };

        var existingPolicies = new[]
        {
            sourcePolicy,
            new RateLimitPolicy { PolicyId = "policy-2", Name = "Login policy (2)" },
            new RateLimitPolicy { PolicyId = "policy-3", Name = "Login policy (3)" },
        };

        var policyStore = new Mock<IRateLimitPolicyStore>();
        policyStore
            .Setup(x => x.FindByIdAsync(policyId, PolicyVersion.Current))
            .Returns(() => ValueTask.FromResult(sourcePolicy));
        policyStore
            .Setup(x => x.GetAllAsync(PolicyVersion.Current))
            .Returns(() => ValueTask.FromResult<IReadOnlyCollection<RateLimitPolicy>>(existingPolicies));

        RateLimitPolicy savedPolicy = null;
        policyStore
            .Setup(x => x.CreateAsync(It.IsAny<RateLimitPolicy>()))
            .Callback<RateLimitPolicy>(policy => savedPolicy = policy)
            .Returns(ValueTask.CompletedTask);

        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var controller = CreateController(policyStore.Object, shellReleaseManager.Object);

        var result = await controller.Clone(policyId);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Edit), redirectResult.ActionName);
        Assert.NotNull(savedPolicy);
        Assert.NotEqual(policyId, savedPolicy.PolicyId);
        Assert.Equal("Login policy (4)", savedPolicy.Name);
        Assert.Equal(sourcePolicy.Description, savedPolicy.Description);
        Assert.Equal(sourcePolicy.Scope, savedPolicy.Scope);
        Assert.Equal(sourcePolicy.Path, savedPolicy.Path);
        Assert.Equal(sourcePolicy.GroupName, savedPolicy.GroupName);
        Assert.False(savedPolicy.IsEnabled);
        Assert.Null(savedPolicy.EnabledUtc);
        Assert.Equal("user-id", savedPolicy.OwnerId);
        Assert.Equal("admin", savedPolicy.Author);
        Assert.Single(savedPolicy.Limiters);
        Assert.NotSame(sourcePolicy.Limiters[0], savedPolicy.Limiters[0]);
        Assert.Equal(sourcePolicy.Limiters[0].Id, savedPolicy.Limiters[0].Id);
        Assert.Equal(sourcePolicy.Limiters[0].Source, savedPolicy.Limiters[0].Source);
        Assert.Equal(sourcePolicy.Limiters[0].Properties["PermitLimit"]?.ToString(), savedPolicy.Limiters[0].Properties["PermitLimit"]?.ToString());
        Assert.NotSame(sourcePolicy.Limiters[0].Properties, savedPolicy.Limiters[0].Properties);
        Assert.Equal(savedPolicy.PolicyId, redirectResult.RouteValues["policyId"]);
        shellReleaseManager.Verify(x => x.RequestRelease(), Times.Never);
    }

    private static AdminController CreateController(
        IRateLimitPolicyStore policyStore,
        IShellReleaseManager shellReleaseManager)
    {
        return new AdminController(
            CreateAuthorizationService().Object,
            Mock.Of<IDisplayManager<RateLimitLimiter>>(),
            Mock.Of<INotifier>(),
            policyStore,
            new ServiceCollection().BuildServiceProvider(),
            shellReleaseManager,
            new EmptyEndpointDataSource(),
            Options.Create(new RateLimitsOptions()),
            Mock.Of<IDisplayManager<RateLimitPolicy>>(),
            Mock.Of<IStringLocalizer<AdminController>>(),
            Mock.Of<IHtmlLocalizer<AdminController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "user-id"),
                        new Claim(ClaimTypes.Name, "admin"),
                    ], "TestAuth")),
                },
            },
        };
    }

    private static Mock<IAuthorizationService> CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService;
    }

    private sealed class EmptyEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => [];

        public override IChangeToken GetChangeToken()
            => NullChangeToken.Singleton;
    }
}
