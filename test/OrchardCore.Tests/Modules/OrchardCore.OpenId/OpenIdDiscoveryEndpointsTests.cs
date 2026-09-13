using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Endpoints.Management;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdDiscoveryEndpointsTests
{
    [Theory]
    [InlineData("AccessRemoteManagement")]
    [InlineData("ManageApplications")]
    public async Task Applications_DeniedPermission_DoNotAccessManager(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var manager = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        var auth = Authorize(denied);

        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.ListApplicationsAsync(http, auth, manager.Object, new())).StatusCode);
        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.GetApplicationAsync(http, auth, manager.Object, "client")).StatusCode);
        manager.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("AccessRemoteManagement")]
    [InlineData("ManageScopes")]
    public async Task Scopes_DeniedPermission_DoNotAccessManager(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var manager = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);
        var auth = Authorize(denied);

        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.ListScopesAsync(http, auth, manager.Object, new())).StatusCode);
        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.GetScopeAsync(http, auth, manager.Object, "scope")).StatusCode);
        manager.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(-1, 50)]
    [InlineData(0, 0)]
    [InlineData(0, 201)]
    public async Task ListAsync_InvalidPaging_DoesNotAccessManagers(int skip, int take)
    {
        var applications = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        var scopes = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);
        var request = new OpenIdListRequest { Skip = skip, Take = take };
        Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.ListApplicationsAsync(new DefaultHttpContext(), Authorize(), applications.Object, request)).StatusCode);
        Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.ListScopesAsync(new DefaultHttpContext(), Authorize(), scopes.Object, request)).StatusCode);
        applications.VerifyNoOtherCalls();
        scopes.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ShowAsync_UnknownIdentifier_ReturnsNotFound()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var applications = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        var scopes = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);
        applications.Setup(service => service.FindByClientIdAsync("missing", default)).ReturnsAsync((object)null);
        scopes.Setup(service => service.FindByNameAsync("missing", default)).ReturnsAsync((object)null);
        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.GetApplicationAsync(http, Authorize(), applications.Object, "missing")).StatusCode);
        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await OpenIdDiscoveryEndpoints.GetScopeAsync(http, Authorize(), scopes.Object, "missing")).StatusCode);
    }

    [Fact]
    public async Task Applications_UseBoundedManagerReadsAndExplicitResponseFields()
    {
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        var http = new DefaultHttpContext { RequestAborted = ct };
        var item = new { Secret = "must-not-appear", Properties = "private", Keys = "private" };
        var manager = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        manager.Setup(service => service.CountAsync(ct)).ReturnsAsync(3L);
        manager.Setup(service => service.ListAsync(1, 2, ct)).Returns(new object[] { item }.ToAsyncEnumerable());
        manager.Setup(service => service.FindByClientIdAsync("client", ct)).ReturnsAsync(item);
        manager.Setup(service => service.GetPhysicalIdAsync(item, ct)).ReturnsAsync("PhysicalId");
        manager.Setup(service => service.GetClientIdAsync(item, ct)).ReturnsAsync("ClientId");
        manager.Setup(service => service.GetDisplayNameAsync(item, ct)).ReturnsAsync("DisplayName");
        manager.Setup(service => service.GetClientTypeAsync(item, ct)).ReturnsAsync("ClientType");
        manager.Setup(service => service.GetApplicationTypeAsync(item, ct)).ReturnsAsync("ApplicationType");
        manager.Setup(service => service.GetConsentTypeAsync(item, ct)).ReturnsAsync("ConsentType");
        manager.Setup(service => service.GetRolesAsync(item, ct)).ReturnsAsync(ImmutableArray.Create("Roles"));
        manager.Setup(service => service.GetPermissionsAsync(item, ct)).ReturnsAsync(ImmutableArray.Create("Permissions"));
        manager.Setup(service => service.GetRequirementsAsync(item, ct)).ReturnsAsync(ImmutableArray.Create("Requirements"));
        manager.Setup(service => service.GetRedirectUrisAsync(item, ct)).ReturnsAsync(ImmutableArray.Create("RedirectUris"));
        manager.Setup(service => service.GetPostLogoutRedirectUrisAsync(item, ct)).ReturnsAsync(ImmutableArray.Create("PostLogoutRedirectUris"));

        var page = Assert.IsType<Ok<OpenIdListResponse<OpenIdApplicationResponse>>>(await OpenIdDiscoveryEndpoints.ListApplicationsAsync(http, Authorize(), manager.Object, new() { Skip = 2, Take = 1 })).Value;
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Skip);
        Assert.Equal(1, page.Take);
        var shown = Assert.IsType<Ok<OpenIdApplicationResponse>>(await OpenIdDiscoveryEndpoints.GetApplicationAsync(http, Authorize(), manager.Object, "client")).Value;
        Assert.Equal(JsonSerializer.Serialize(Assert.Single(page.Items)), JsonSerializer.Serialize(shown));
        var json = JsonSerializer.Serialize(shown);
        Assert.DoesNotContain("must-not-appear", json, StringComparison.Ordinal);
        Assert.DoesNotContain("private", json, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(new[] { "Id", "ClientId", "DisplayName", "ClientType", "ApplicationType", "ConsentType", "Roles", "Permissions", "Requirements", "RedirectUris", "PostLogoutRedirectUris" },
            document.RootElement.EnumerateObject().Select(property => property.Name));
        manager.Verify(service => service.ListAsync(1, 2, ct), Times.Once);
    }

    [Fact]
    public async Task Scopes_UseBoundedManagerReadsAndExplicitResponseFields()
    {
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        var http = new DefaultHttpContext { RequestAborted = ct };
        var item = new { Secret = "must-not-appear", Properties = "private", Keys = "private" };
        var manager = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);
        manager.Setup(service => service.CountAsync(ct)).ReturnsAsync(3L);
        manager.Setup(service => service.ListAsync(1, 2, ct)).Returns(new object[] { item }.ToAsyncEnumerable());
        manager.Setup(service => service.FindByNameAsync("scope", ct)).ReturnsAsync(item);
        manager.Setup(service => service.GetPhysicalIdAsync(item, ct)).ReturnsAsync("PhysicalId");
        manager.Setup(service => service.GetNameAsync(item, ct)).ReturnsAsync("Name");
        manager.Setup(service => service.GetDisplayNameAsync(item, ct)).ReturnsAsync("DisplayName");
        manager.Setup(service => service.GetDescriptionAsync(item, ct)).ReturnsAsync("Description");
        manager.Setup(service => service.GetResourcesAsync(item, ct)).ReturnsAsync(ImmutableArray.Create("Resources"));

        var page = Assert.IsType<Ok<OpenIdListResponse<OpenIdScopeResponse>>>(await OpenIdDiscoveryEndpoints.ListScopesAsync(http, Authorize(), manager.Object, new() { Skip = 2, Take = 1 })).Value;
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Skip);
        Assert.Equal(1, page.Take);
        var shown = Assert.IsType<Ok<OpenIdScopeResponse>>(await OpenIdDiscoveryEndpoints.GetScopeAsync(http, Authorize(), manager.Object, "scope")).Value;
        Assert.Equal(JsonSerializer.Serialize(Assert.Single(page.Items)), JsonSerializer.Serialize(shown));
        var json = JsonSerializer.Serialize(shown);
        Assert.DoesNotContain("must-not-appear", json, StringComparison.Ordinal);
        Assert.DoesNotContain("private", json, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(new[] { "Id", "Name", "DisplayName", "Description", "Resources" },
            document.RootElement.EnumerateObject().Select(property => property.Name));
        manager.Verify(service => service.ListAsync(1, 2, ct), Times.Once);
    }

    private static IAuthorizationService Authorize(string denied = null)
    {
        var auth = new Mock<IAuthorizationService>();
        auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().Any(requirement => requirement.Permission.Name == denied)
                    ? AuthorizationResult.Failed() : AuthorizationResult.Success());
        return auth.Object;
    }
}
