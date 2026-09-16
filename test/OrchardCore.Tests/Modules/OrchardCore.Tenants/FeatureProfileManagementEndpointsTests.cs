using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tenants.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class FeatureProfileManagementEndpointsTests
{
    [Theory]
    [InlineData("Default", "AccessRemoteManagement")]
    [InlineData("Default", "ManageTenantFeatureProfiles")]
    [InlineData("Child", null)]
    public async Task Operations_Unauthorized_DoNotAccessProfileServices(string tenant, string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var shell = new ShellSettings { Name = tenant };
        var authorization = Authorize(denied);
        Assert.Equal(403, Status(await FeatureProfileManagementEndpoints.ListAsync(http, authorization, shell, null, new())));
        Assert.Equal(403, Status(await FeatureProfileManagementEndpoints.GetAsync(http, authorization, shell, null, "profile")));
        Assert.Equal(403, Status(await FeatureProfileManagementEndpoints.SchemaAsync(http, authorization, shell, null)));
        Assert.Equal(403, Status(await FeatureProfileManagementEndpoints.CreateAsync(http, authorization, shell, null, Definition())));
        Assert.Equal(403, Status(await FeatureProfileManagementEndpoints.UpdateAsync(http, authorization, shell, null, "profile", Definition())));
        Assert.Equal(403, Status(await FeatureProfileManagementEndpoints.DeleteAsync(http, authorization, shell, null, "profile")));
    }

    [Fact]
    public async Task Operations_CrudRetriesAndValidation_UseSharedDocument()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.PathBase = "/host";
        var shell = new ShellSettings { Name = ShellSettings.DefaultShellName };
        var authorization = Authorize();
        var document = new FeatureProfilesDocument();
        var manager = FeatureProfilesManagerTests.CreateManager(document, out _);
        var created = Assert.IsType<Created<FeatureProfileDefinition>>(await FeatureProfileManagementEndpoints.CreateAsync(http, authorization, shell, manager, Definition()));
        Assert.Equal("/host/api/tenants/feature-profiles/by-id?id=profile", created.Location);
        Assert.Equal(200, Status(await FeatureProfileManagementEndpoints.CreateAsync(http, authorization, shell, manager, Definition())));
        Assert.Equal(409, Status(await FeatureProfileManagementEndpoints.CreateAsync(http, authorization, shell, manager, Definition("Changed"))));
        Assert.Equal(400, Status(await FeatureProfileManagementEndpoints.ListAsync(http, authorization, shell, manager, new() { Take = 201 })));
        var list = Assert.IsType<Ok<FeatureProfileListResponse>>(await FeatureProfileManagementEndpoints.ListAsync(http, authorization, shell, manager, new() { Search = "PROF" }));
        Assert.Equal(1, list.Value.TotalCount);
        Assert.Single(list.Value.Items);
        Assert.Equal(400, Status(await FeatureProfileManagementEndpoints.UpdateAsync(http, authorization, shell, manager, "different", Definition())));
        Assert.Equal(400, Status(await FeatureProfileManagementEndpoints.UpdateAsync(http, authorization, shell, manager, "profile",
            new() { Id = "profile", Name = "Invalid", FeatureRules = null })));
        Assert.Equal("Profile", document.FeatureProfiles["profile"].Name);
        Assert.Equal(200, Status(await FeatureProfileManagementEndpoints.UpdateAsync(http, authorization, shell, manager, "profile", Definition("Renamed"))));
        Assert.Equal("Renamed", document.FeatureProfiles["profile"].Name);
        Assert.IsType<NoContent>(await FeatureProfileManagementEndpoints.DeleteAsync(http, authorization, shell, manager, "profile"));
        Assert.IsType<NoContent>(await FeatureProfileManagementEndpoints.DeleteAsync(http, authorization, shell, manager, "profile"));
        Assert.Equal(404, Status(await FeatureProfileManagementEndpoints.GetAsync(http, authorization, shell, manager, "profile")));
        Assert.Equal(404, Status(await FeatureProfileManagementEndpoints.UpdateAsync(http, authorization, shell, manager, "profile", Definition())));
    }

    private static FeatureProfileDefinition Definition(string name = "Profile") => new()
    {
        Id = "profile", Name = name,
        FeatureRules = [new() { Rule = "Exclude", Expression = "Custom.*" }],
    };

    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;

    private static IAuthorizationService Authorize(string denied = null)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().Any(requirement => requirement.Permission.Name == denied)
                    ? AuthorizationResult.Failed() : AuthorizationResult.Success());
        return authorization.Object;
    }
}
