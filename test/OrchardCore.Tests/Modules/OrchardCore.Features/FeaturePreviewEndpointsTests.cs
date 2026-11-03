using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Endpoints.Management;
using OrchardCore.Features.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Features;

public class FeaturePreviewEndpointsTests
{
    [Fact]
    public async Task Preview_EnableWithOneMissingFeature_IsAllowedWithoutForce()
    {
        var fixture = new Fixture();
        fixture.Add("Dependency", enabled: true);
        fixture.Add("Target", enabled: false, dependencies: ["Dependency"]);

        var response = await fixture.PreviewAsync("Target", "enable");

        Assert.True(response.IsAllowed);
        Assert.False(response.RequiresForce);
        Assert.Equal(["Target"], response.FeaturesToEnable);
        Assert.Empty(response.FeaturesToDisable);
        fixture.VerifyNothingChanged();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Preview_EnableWithMissingDependency_RequiresForce(bool force, bool allowed)
    {
        var fixture = new Fixture();
        fixture.Add("Dependency", enabled: false);
        fixture.Add("Target", enabled: false, dependencies: ["Dependency"]);

        var response = await fixture.PreviewAsync("Target", "enable", force);

        Assert.Equal(allowed, response.IsAllowed);
        Assert.True(response.RequiresForce);
        Assert.Equal(["Dependency", "Target"], response.FeaturesToEnable);
        fixture.VerifyNothingChanged();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Preview_DisableWithEnabledDependent_RequiresForce(bool force, bool allowed)
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled: true);
        fixture.Add("Dependent", enabled: true, dependencies: ["Target"]);
        fixture.Add("Disabled", enabled: false, dependencies: ["Target"]);

        var response = await fixture.PreviewAsync("Target", "disable", force);

        Assert.Equal(allowed, response.IsAllowed);
        Assert.True(response.RequiresForce);
        Assert.Equal(["Dependent", "Target"], response.FeaturesToDisable);
        fixture.VerifyNothingChanged();
    }

    [Fact]
    public async Task Preview_DisableLastDependent_ListsTheUnneededDependencyOnlyFeature()
    {
        var fixture = new Fixture();
        fixture.Add("Hidden", enabled: true, byDependencyOnly: true);
        fixture.Add("Target", enabled: true, dependencies: ["Hidden"]);

        var response = await fixture.PreviewAsync("Target", "disable");

        Assert.True(response.IsAllowed);
        Assert.Equal(["Target"], response.FeaturesToDisable);
        Assert.Equal(["Hidden"], response.DependencyOnlyFeaturesToDisable);
    }

    [Fact]
    public async Task Preview_DisableAlwaysEnabledFeature_IsBlocked()
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled: true, alwaysEnabled: true);

        var response = await fixture.PreviewAsync("Target", "disable", force: true);

        Assert.False(response.IsAllowed);
        Assert.NotNull(response.BlockedReason);
        Assert.Empty(response.FeaturesToDisable);
    }

    [Theory]
    [InlineData("enable", false)]
    [InlineData("disable", true)]
    public async Task Preview_DependencyOnlyFeature_IsBlocked(string action, bool enabled)
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled, byDependencyOnly: true);

        var response = await fixture.PreviewAsync("Target", action, force: true);

        Assert.False(response.IsAllowed);
        Assert.NotNull(response.BlockedReason);
    }

    [Fact]
    public async Task Preview_AlreadyInRequestedState_ChangesNothing()
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled: true);

        var response = await fixture.PreviewAsync("Target", "enable");

        Assert.True(response.IsAllowed);
        Assert.Empty(response.FeaturesToEnable);
        Assert.Empty(response.FeaturesToDisable);
    }

    [Fact]
    public async Task Preview_UnknownActionOrFeature_IsAProblem()
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled: true);

        var badAction = Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InvokeAsync("Target", "remove"));
        Assert.Equal(StatusCodes.Status400BadRequest, badAction.StatusCode);

        var missing = Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InvokeAsync("Missing", "enable"));
        Assert.Equal(StatusCodes.Status404NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Preview_WithoutManageFeatures_IsForbidden()
    {
        var fixture = new Fixture { Authorized = false };
        fixture.Add("Target", enabled: true);

        var forbidden = Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InvokeAsync("Target", "enable"));

        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Disable_FeatureEnabledByDependencyOnly_IsRefusedLikeThePreview()
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled: true, byDependencyOnly: true);
        fixture.Add("Dependent", enabled: true, dependencies: ["Target"]);

        Assert.False((await fixture.PreviewAsync("Target", "disable")).IsAllowed);

        var result = Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.ChangeAsync("Target", enable: false));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        fixture.VerifyNothingChanged();
    }

    [Fact]
    public async Task Enable_FeatureEnabledByDependencyOnly_IsRefusedLikeThePreview()
    {
        var fixture = new Fixture();
        fixture.Add("Target", enabled: false, byDependencyOnly: true);

        Assert.False((await fixture.PreviewAsync("Target", "enable")).IsAllowed);

        var result = Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.ChangeAsync("Target", enable: true));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        fixture.VerifyNothingChanged();
    }

    private sealed class Fixture
    {
        private readonly List<IFeatureInfo> _features = [];
        private readonly HashSet<string> _enabled = [];
        private readonly HashSet<string> _alwaysEnabled = [];
        private readonly Dictionary<string, string[]> _dependencies = [];
        private readonly Mock<IShellFeaturesManager> _manager = new();

        public bool Authorized { get; init; } = true;

        public void Add(string id, bool enabled, string[] dependencies = null, bool byDependencyOnly = false, bool alwaysEnabled = false)
        {
            _features.Add(Mock.Of<IFeatureInfo>(feature =>
                feature.Id == id && feature.EnabledByDependencyOnly == byDependencyOnly && feature.IsAlwaysEnabled == alwaysEnabled));
            _dependencies[id] = dependencies ?? [];

            if (enabled)
            {
                _enabled.Add(id);
            }

            if (alwaysEnabled)
            {
                _alwaysEnabled.Add(id);
            }
        }

        public void VerifyNothingChanged()
            => _manager.Verify(
                manager => manager.UpdateFeaturesAsync(It.IsAny<IEnumerable<IFeatureInfo>>(), It.IsAny<IEnumerable<IFeatureInfo>>(), It.IsAny<bool>()),
                Times.Never);

        public async Task<FeaturePreviewEndpoints.FeaturePreviewResponse> PreviewAsync(string id, string action, bool force = false)
            => Assert.IsType<Ok<FeaturePreviewEndpoints.FeaturePreviewResponse>>(await InvokeAsync(id, action, force)).Value;

        public Task<IResult> InvokeAsync(string id, string action, bool force = false)
        {
            _manager.Setup(manager => manager.GetAvailableFeaturesAsync()).ReturnsAsync(_features);
            _manager.Setup(manager => manager.GetEnabledFeaturesAsync()).ReturnsAsync(() => _features.Where(feature => _enabled.Contains(feature.Id)));
            _manager.Setup(manager => manager.GetAlwaysEnabledFeaturesAsync()).ReturnsAsync(() => _features.Where(feature => _alwaysEnabled.Contains(feature.Id)));

            var extensions = new Mock<IExtensionManager>();
            extensions.Setup(manager => manager.GetFeatureDependencies(It.IsAny<string>())).Returns((string featureId) => Dependencies(featureId));
            extensions.Setup(manager => manager.GetDependentFeatures(It.IsAny<string>())).Returns((string featureId) => Dependents(featureId));

            var authorization = new Mock<IAuthorizationService>();
            authorization
                .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(Authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

            var services = new ServiceCollection()
                .AddSingleton<IStringLocalizer<ProblemDetailsApiLocalization>>(new StringLocalizer<ProblemDetailsApiLocalization>(new global::OrchardCore.Localization.NullStringLocalizerFactory()))
                .BuildServiceProvider();

            return FeaturePreviewEndpoints.PreviewAsync(
                new DefaultHttpContext { RequestServices = services },
                id,
                authorization.Object,
                new FeatureService(_manager.Object, extensions.Object),
                _manager.Object,
                extensions.Object,
                new StringLocalizer<global::OrchardCore.Features.Permissions>(new global::OrchardCore.Localization.NullStringLocalizerFactory()),
                action,
                force);
        }

        public async Task<IResult> ChangeAsync(string id, bool enable)
        {
            // Sets up the shared mocks.
            await InvokeAsync(id, "enable");

            var extensions = new Mock<IExtensionManager>();
            var authorization = new Mock<IAuthorizationService>();
            authorization
                .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var services = new ServiceCollection()
                .AddSingleton<IStringLocalizer<ProblemDetailsApiLocalization>>(new StringLocalizer<ProblemDetailsApiLocalization>(new global::OrchardCore.Localization.NullStringLocalizerFactory()))
                .BuildServiceProvider();
            var context = new DefaultHttpContext { RequestServices = services };
            var featureService = new FeatureService(_manager.Object, extensions.Object);
            var localizer = new StringLocalizer<global::OrchardCore.Features.Permissions>(new global::OrchardCore.Localization.NullStringLocalizerFactory());

            return enable
                ? await FeatureManagementEndpoints.EnableAsync(context, id, authorization.Object, featureService, _manager.Object, localizer)
                : await FeatureManagementEndpoints.DisableAsync(context, id, authorization.Object, featureService, _manager.Object, localizer);
        }

        // Like the extension manager: dependencies first, then the feature.
        private IEnumerable<IFeatureInfo> Dependencies(string id)
            => _dependencies[id].SelectMany(Dependencies).Append(_features.Single(feature => feature.Id == id)).Distinct();

        // Like the extension manager: the feature, then its dependents.
        private IEnumerable<IFeatureInfo> Dependents(string id)
            => _features.Where(feature => feature.Id == id || Dependencies(feature.Id).Any(dependency => dependency.Id == id));
    }
}
