using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Zones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Drivers;
using OrchardCore.RateLimits.Endpoints;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.RateLimits.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimitManagementTests
{
    [Fact]
    public async Task Endpoints_ExposeUniqueCommandsAndRequireBearerAndBothPermissions()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton(Mock.Of<IRateLimitPolicyStore>());
        builder.Services.AddSingleton(Mock.Of<IShellReleaseManager>());
        builder.Services.AddScoped<RateLimitPolicyMutations>();
        builder.Services.AddScoped<RateLimitLimiterMutations>();
        builder.Services.AddSingleton<IStringLocalizer<RateLimitLimiterInput>>(Localizer());
        await using var app = builder.Build();
        ((IEndpointRouteBuilder)app).AddRateLimitEndpoints();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray();
        Assert.Equal(13, endpoints.Length);
        Assert.Equal(13, endpoints.Select(endpoint => endpoint.Metadata.GetRequiredMetadata<IEndpointNameMetadata>().EndpointName).Distinct().Count());
        foreach (var endpoint in endpoints)
        {
            Assert.Equal("rate-limits", endpoint.Metadata.GetRequiredMetadata<CliOperationMetadata>().Capability);
            var policy = endpoint.Metadata.GetRequiredMetadata<AuthorizationPolicy>();
            Assert.Contains(OrchardCoreConstants.AuthenticationSchemes.Api, policy.AuthenticationSchemes);
            var permissions = policy.Requirements.OfType<global::OrchardCore.Security.PermissionRequirement>().ToArray();
            Assert.Equal(new[] { "AccessRemoteManagement", "ManageRateLimits" }, permissions.Select(value => value.Permission.Name).Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public async Task PolicyCreate_RetryAndConflict_DoNotOverwriteExistingState()
    {
        var policy = Policy();
        var store = Store(policy);
        var mutations = new RateLimitPolicyMutations(store.Object, Mock.Of<IShellReleaseManager>());
        var input = new RateLimitPolicyInput { Name = " Policy ", Scope = "Endpoint", Path = "/limited" };
        var retry = Assert.IsType<Created<RateLimitPolicyResponse>>(await RateLimitEndpoints.CreateAsync(new DefaultHttpContext(),
            Authorization(), store.Object, mutations, input));
        Assert.Equal("policy", retry.Value.PolicyId);
        input.Path = "/other";
        Assert.Equal(409, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.CreateAsync(new DefaultHttpContext(),
            Authorization(), store.Object, mutations, input)).StatusCode);
        store.Verify(value => value.CreateAsync(It.IsAny<RateLimitPolicy>()), Times.Never);
        Assert.Equal("/limited", policy.Path);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task EmptyPolicyId_IsRejectedBeforeStoreAccess(string policyId)
    {
        var store = new Mock<IRateLimitPolicyStore>(MockBehavior.Strict);
        Assert.Equal(400, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.GetAsync(new DefaultHttpContext(),
            Authorization(), store.Object, policyId)).StatusCode);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LimiterUpdate_InvalidOrEnabled_DoesNotChangeStoredSettings()
    {
        var policy = Policy();
        var store = Store(policy);
        var mutations = new RateLimitLimiterMutations(store.Object);
        var before = policy.Limiters[0].Properties.DeepClone();
        var input = Input();
        input.Values["queueLimit"] = -1;
        Assert.IsType<ValidationProblem>(await RateLimitEndpoints.UpdateLimiterAsync(new DefaultHttpContext(), Authorization(),
            store.Object, mutations, Localizer(), "policy", "limiter", input));
        Assert.True(JsonNode.DeepEquals(before, policy.Limiters[0].Properties));
        input.Values["queueLimit"] = 0;
        input.Values["permitLimit"] = 3;
        policy.IsEnabled = true;
        Assert.Equal(409, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.UpdateLimiterAsync(new DefaultHttpContext(), Authorization(),
            store.Object, mutations, Localizer(), "policy", "limiter", input)).StatusCode);
        Assert.True(JsonNode.DeepEquals(before, policy.Limiters[0].Properties));
        store.Verify(value => value.UpdateAsync(It.IsAny<RateLimitPolicy>()), Times.Never);
    }

    [Fact]
    public async Task LimiterRetry_IsUnchangedAndUpdatesPreserveUncontractedProperties()
    {
        var policy = Policy();
        policy.Limiters[0].Properties["Extension"] = new JsonObject { ["keep"] = true };
        var store = Store(policy);
        var mutations = new RateLimitLimiterMutations(store.Object);
        var result = await RateLimitEndpoints.AddLimiterAsync(new DefaultHttpContext(), Authorization(), store.Object, mutations, Localizer(), "policy", Input());
        Assert.IsType<Ok<RateLimitLimiterResponse>>(result);
        store.Verify(value => value.UpdateAsync(It.IsAny<RateLimitPolicy>()), Times.Never);
        var input = Input();
        input.Values["permitLimit"] = 3;
        Assert.Equal(409, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.AddLimiterAsync(new DefaultHttpContext(), Authorization(),
            store.Object, mutations, Localizer(), "policy", input)).StatusCode);
        var updated = Assert.IsType<Ok<RateLimitLimiterResponse>>(await RateLimitEndpoints.UpdateLimiterAsync(new DefaultHttpContext(), Authorization(),
            store.Object, mutations, Localizer(), "policy", "limiter", input));
        Assert.Equal(3, updated.Value.Values["permitLimit"].GetValue<int>());
        Assert.True(policy.Limiters[0].Properties["Extension"]["keep"].GetValue<bool>());
        Assert.False(updated.Value.Values.ContainsKey("Extension"));
        store.Verify(value => value.UpdateAsync(policy), Times.Once);
    }

    [Fact]
    public async Task PolicyTargetAndStatus_RequireDisableAndReloadOnlyOnStatusChange()
    {
        var policy = Policy();
        policy.IsEnabled = true;
        var store = Store(policy);
        store.Setup(value => value.SetStatusAsync("policy", false)).Returns(() =>
        {
            policy.IsEnabled = false;
            return ValueTask.FromResult(true);
        });
        var release = new Mock<IShellReleaseManager>();
        var mutations = new RateLimitPolicyMutations(store.Object, release.Object);
        var input = new RateLimitPolicyInput { Name = "Policy", Scope = "Endpoint", Path = "/new" };
        Assert.Equal(409, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.UpdateAsync(new DefaultHttpContext(), Authorization(),
            store.Object, mutations, "policy", input)).StatusCode);
        Assert.Equal("/limited", policy.Path);
        input.Path = policy.Path;
        input.Description = "New description";
        Assert.IsType<Ok<RateLimitPolicyResponse>>(await RateLimitEndpoints.UpdateAsync(new DefaultHttpContext(), Authorization(), store.Object, mutations, "policy", input));
        release.Verify(value => value.RequestRelease(), Times.Never);
        await RateLimitEndpoints.DisableAsync(new DefaultHttpContext(), Authorization(), store.Object, mutations, "policy");
        await RateLimitEndpoints.DisableAsync(new DefaultHttpContext(), Authorization(), store.Object, mutations, "policy");
        release.Verify(value => value.RequestRelease(), Times.Once);
        store.Verify(value => value.SetStatusAsync("policy", false), Times.Once);
    }

    [Fact]
    public async Task DeniedOperations_DoNotReadOrWritePolicies()
    {
        var store = new Mock<IRateLimitPolicyStore>(MockBehavior.Strict);
        using var services = new ServiceCollection().AddSingleton<IStringLocalizer<ProblemDetailsApiLocalization>>(
            new StringLocalizer<ProblemDetailsApiLocalization>(new NullStringLocalizerFactory())).BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        var authorization = Authorization(false);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.ListAsync(context, authorization, store.Object, null, null)).StatusCode);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.AddLimiterAsync(context, authorization, store.Object,
            new RateLimitLimiterMutations(store.Object), Localizer(), "policy", Input())).StatusCode);
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(await RateLimitEndpoints.DeleteAsync(context, authorization, store.Object,
            new RateLimitPolicyMutations(store.Object, Mock.Of<IShellReleaseManager>()), "policy")).StatusCode);
        store.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("FixedWindow", "{\"permitLimit\":1,\"windowSeconds\":1,\"queueLimit\":0}")]
    [InlineData("SlidingWindow", "{\"permitLimit\":1,\"windowSeconds\":1,\"segmentsPerWindow\":1,\"queueLimit\":0}")]
    [InlineData("Concurrency", "{\"permitLimit\":1,\"queueLimit\":0,\"queueProcessingOrder\":\"OldestFirst\"}")]
    [InlineData("TokenBucket", "{\"tokenLimit\":1,\"tokensPerPeriod\":1,\"replenishmentPeriodSeconds\":1,\"queueLimit\":0,\"queueProcessingOrder\":\"NewestFirst\"}")]
    public void SourceContracts_ValidateCompleteSettingsBeforeMutation(string source, string json)
    {
        var limiter = new RateLimitLimiter { Source = source };
        var values = JsonNode.Parse(json).AsObject();
        Assert.Empty(RateLimitLimiterConfiguration.Configure(limiter, values, Localizer()));
        var before = limiter.Properties.DeepClone();
        values["queueLimit"] = -1;
        Assert.Contains("values.queueLimit", RateLimitLimiterConfiguration.Configure(limiter, values, Localizer()).Keys);
        Assert.True(JsonNode.DeepEquals(before, limiter.Properties));
        values["queueLimit"] = 0;
        values["unknown"] = true;
        Assert.NotEmpty(RateLimitLimiterConfiguration.Configure(limiter, values, Localizer()));
        Assert.True(JsonNode.DeepEquals(before, limiter.Properties));
    }

    [Fact]
    public async Task FixedWindowEditor_RejectsNegativeQueuesUsingTheSameValidationAsApi()
    {
        var updater = new Mock<IUpdateModel>();
        var state = new ModelStateDictionary();
        updater.SetupGet(value => value.ModelState).Returns(state);
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<FixedWindowRateLimiterViewModel>(), It.IsAny<string>()))
            .Callback<FixedWindowRateLimiterViewModel, string>((model, _) =>
            {
                model.PermitLimit = 1; model.WindowSeconds = 1; model.QueueLimit = -1;
            }).ReturnsAsync(true);
        var driver = new FixedWindowRateLimiterDisplayDriver(null,
            new StringLocalizer<FixedWindowRateLimiterDisplayDriver>(new NullStringLocalizerFactory()));
        await driver.UpdateAsync(new RateLimitLimiter { Source = "FixedWindow" },
            new UpdateEditorContext(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));
        Assert.False(state.IsValid);
        Assert.Contains(state.Keys, key => key.EndsWith("QueueLimit", StringComparison.Ordinal));
    }

    private static RateLimitPolicy Policy()
    {
        var limiter = new RateLimitLimiter { Id = "limiter", Source = "FixedWindow" };
        limiter.Put(new FixedWindowRateLimiterData { PermitLimit = 2, WindowSeconds = 60 });
        return new RateLimitPolicy { PolicyId = "policy", Name = "Policy", Scope = RateLimitPolicyScope.Endpoint, Path = "/limited", Limiters = [limiter] };
    }
    private static RateLimitLimiterInput Input() => new()
    {
        Id = "limiter", Source = "FixedWindow", Values = new JsonObject { ["permitLimit"] = 2, ["windowSeconds"] = 60, ["queueLimit"] = 0 },
    };
    private static StringLocalizer<RateLimitLimiterInput> Localizer() => new(new NullStringLocalizerFactory());
    private static Mock<IRateLimitPolicyStore> Store(RateLimitPolicy policy)
    {
        var store = new Mock<IRateLimitPolicyStore>();
        store.Setup(value => value.FindByIdAsync(policy.PolicyId, PolicyVersion.Current)).Returns(() => ValueTask.FromResult(policy));
        store.Setup(value => value.GetAllAsync(PolicyVersion.Current)).Returns(() => ValueTask.FromResult<IReadOnlyCollection<RateLimitPolicy>>([policy]));
        return store;
    }
    private static IAuthorizationService Authorization(bool allowed = true)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
