using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Artifacts;
using OrchardCore.Deployment.Operations;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentOperationExecutorTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Authorize_NoCapturedIdentity_IsRefused(bool import)
    {
        var kind = import ? DeploymentOperationKind.Import : DeploymentOperationKind.Export;
        var operation = new DeploymentOperation { Kind = kind, Owner = "owner", Payload = "payload" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            DeploymentOperationExecutor.AuthorizeCallerAsync(Authorize(), operation));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Authorize_CallerLostThePermission_IsRefused(bool import)
    {
        var kind = import ? DeploymentOperationKind.Import : DeploymentOperationKind.Export;
        var other = import ? DeploymentPermissions.Export : DeploymentPermissions.Import;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            DeploymentOperationExecutor.AuthorizeCallerAsync(Authorize(other), Operation(Caller(), kind, "payload")));
    }

    [Fact]
    public async Task Authorize_OwnerDiffersFromCapturedIdentity_IsRefused()
    {
        var operation = Operation(Caller(), DeploymentOperationKind.Import, "payload") with { Owner = "another" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            DeploymentOperationExecutor.AuthorizeCallerAsync(Authorize(DeploymentPermissions.Import), operation));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Authorize_CallerHasThePermission_ReturnsTheCapturedIdentity(bool import)
    {
        var kind = import ? DeploymentOperationKind.Import : DeploymentOperationKind.Export;
        var permission = import ? DeploymentPermissions.Import : DeploymentPermissions.Export;

        var principal = await DeploymentOperationExecutor.AuthorizeCallerAsync(Authorize(permission), Operation(Caller(), kind, "payload"));

        Assert.Equal("caller", principal.FindFirst("sub").Value);
    }

    [Fact]
    public async Task Import_AuthorizedCaller_RunsAndRestoresTheCurrentUser()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var token = TestContext.Current.CancellationToken;
            var services = scope.ServiceProvider;
            var principal = Caller();
            using var input = new MemoryStream(Encoding.UTF8.GetBytes("""{"name":"empty","steps":[]}"""));
            var artifact = await services.GetRequiredService<DeploymentArtifactStore>().CreateAsync(
                DeploymentArtifactOwner.Get(principal), DeploymentArtifactKind.Import, "Recipe.json", "application/json", input, token);
            var accessor = services.GetRequiredService<IHttpContextAccessor>();
            var user = accessor.HttpContext?.User;

            await services.GetRequiredService<IDeploymentOperationExecutor>().ExecuteAsync(Operation(principal, DeploymentOperationKind.Import, artifact.Id), token);

            Assert.Same(user, accessor.HttpContext?.User);
        });
    }

    private static DeploymentOperation Operation(ClaimsPrincipal principal, DeploymentOperationKind kind, string payload) => new()
    {
        Kind = kind,
        Owner = DeploymentArtifactOwner.Get(principal),
        Payload = payload,
        Identity = DeploymentExecutionIdentity.Capture(principal),
    };

    private static ClaimsPrincipal Caller() => new(new ClaimsIdentity(
        [new Claim("sub", "caller"), new Claim("oc:entyp", "application")], "Bearer"));

    private static IAuthorizationService Authorize(params Permission[] permissions)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => permissions.Any(permission => permission.Name == requirement.Permission.Name))
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
