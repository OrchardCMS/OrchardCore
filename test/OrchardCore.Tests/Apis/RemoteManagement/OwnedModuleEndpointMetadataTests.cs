using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Features.Endpoints.Management;
using OrchardCore.Features.Services;
using OrchardCore.HomeRoute.Endpoints;
using OrchardCore.Modules;
using OrchardCore.Recipes.Endpoints.Management;
using OrchardCore.Recipes.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Roles.Endpoints.Management;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tenants;
using OrchardCore.Tenants.Services;
using OrchardCore.Themes.Endpoints.Management;
using OrchardCore.Users.Endpoints.Management;
using OrchardCore.Users.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class OwnedModuleEndpointMetadataTests
{
    [Fact]
    public void Endpoints_ExposeOwnedManagementMetadata()
    {
        var endpoints = CreateEndpoints();

        AssertOperation(endpoints, "api/tenants", "GET", "ApiListTenants", "Tenants", ["tenants"], "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/tenants/{tenantName}", "GET", "ApiGetTenant", "Tenants", ["tenants"], "show", null, [200, 401, 403, 404]);
        AssertOperation(endpoints, "api/tenants", "POST", "ApiCreateTenantManagement", "Tenants", ["tenants"], "create", "application/json", [200, 201, 400, 401, 403, 409]);
        AssertOperation(endpoints, "api/tenants/{tenantName}:install", "POST", "ApiInstallTenantManagement", "Tenants", ["tenants"], "install", "application/json", [201, 400, 401, 403, 404, 409, 500]);
        AssertOperation(endpoints, "api/tenants/{tenantName}:setup", "POST", "ApiSetupTenantManagement", "Tenants", ["tenants"], "setup", "application/json", [200, 400, 401, 403, 404, 409, 500]);
        AssertOperation(endpoints, "api/tenants/{tenantName}", "PUT", "ApiUpdateTenantManagement", "Tenants", ["tenants"], "update", "application/json", [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/tenants/{tenantName}", "DELETE", "ApiDeleteTenant", "Tenants", ["tenants"], "delete", null, [200, 204, 400, 401, 403]);
        AssertOperation(endpoints, "api/tenants/{tenantName}:start", "POST", "ApiStartTenant", "Tenants", ["tenants"], "start", null, [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/tenants/{tenantName}:stop", "POST", "ApiStopTenant", "Tenants", ["tenants"], "stop", null, [200, 400, 401, 403, 404]);
        Assert.Equal(
            CliInputMode.Options,
            endpoints.Single(endpoint =>
                string.Equals(endpoint.RoutePattern.RawText, "api/tenants", StringComparison.Ordinal) &&
                endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST", StringComparer.OrdinalIgnoreCase) == true)
                .Metadata.GetRequiredMetadata<CliOperationMetadata>()
                .InputMode);
        var setupMetadata = endpoints.Single(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText, "api/tenants/{tenantName}:setup", StringComparison.Ordinal))
            .Metadata.GetRequiredMetadata<CliOperationMetadata>();
        Assert.Equal(["password", "connectionString"], setupMetadata.SecretProperties);
        var installMetadata = endpoints.Single(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText, "api/tenants/{tenantName}:install", StringComparison.Ordinal))
            .Metadata.GetRequiredMetadata<CliOperationMetadata>();
        Assert.Equal(["password", "connectionString"], installMetadata.SecretProperties);


        AssertOperation(endpoints, "api/features", "GET", "ApiListFeatures", "Features", ["features"], "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/features/{featureId}", "GET", "ApiGetFeature", "Features", ["features"], "show", null, [200, 401, 403, 404]);
        AssertOperation(endpoints, "api/features/{featureId}:enable", "POST", "ApiEnableFeature", "Features", ["features"], "enable", null, [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/features/{featureId}:disable", "POST", "ApiDisableFeature", "Features", ["features"], "disable", null, [200, 400, 401, 403, 404]);

        AssertOperation(endpoints, "api/recipes", "GET", "ApiListRecipes", "Recipes", ["recipes"], "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/recipes/{recipeId}", "GET", "ApiGetRecipe", "Recipes", ["recipes"], "show", null, [200, 401, 403, 404]);
        AssertOperation(endpoints, "api/recipes/{recipeId}:execute", "POST", "ApiExecuteRecipe", "Recipes", ["recipes"], "execute", "application/json", [200, 400, 401, 403, 404]);

        AssertOperation(endpoints, "api/users", "GET", "ApiListUsers", "Users", ["users"], "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/users/{userId}", "GET", "ApiGetUser", "Users", ["users"], "show", null, [200, 401, 403, 404]);
        AssertOperation(endpoints, "api/users", "POST", "ApiCreateUser", "Users", ["users"], "create", "application/json", [200, 201, 400, 401, 403, 409]);
        AssertOperation(endpoints, "api/users/{userId}", "PUT", "ApiUpdateUser", "Users", ["users"], "update", "application/json", [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/users/{userId}:disable", "POST", "ApiDisableUser", "Users", ["users"], "disable", null, [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/users/{userId}", "DELETE", "ApiDeleteUser", "Users", ["users"], "delete", null, [200, 204, 400, 401, 403]);

        AssertOperation(endpoints, "api/roles", "GET", "ApiListRoles", "Roles", ["roles"], "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/roles/{roleId}", "GET", "ApiGetRole", "Roles", ["roles"], "show", null, [200, 401, 403, 404]);
        AssertOperation(endpoints, "api/roles", "POST", "ApiCreateRole", "Roles", ["roles"], "create", "application/json", [201, 400, 401, 403, 409]);
        AssertOperation(endpoints, "api/roles/{roleId}", "PUT", "ApiUpdateRole", "Roles", ["roles"], "update", "application/json", [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/roles/{roleId}", "DELETE", "ApiDeleteRole", "Roles", ["roles"], "delete", null, [200, 400, 401, 403]);

        AssertOperation(endpoints, "api/themes", "GET", "ApiListThemes", "Themes", ["themes"], "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/themes/{themeId}:enable", "POST", "ApiEnableTheme", "Themes", ["themes"], "enable", null, [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/themes/{themeId}:set-current", "POST", "ApiSetCurrentTheme", "Themes", ["themes"], "set-current", null, [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/home-route/content/{contentItemId}", "PUT", "ApiSetHomeContent", "Home Route", ["settings"], "set-home-content", null, [200, 401, 403, 404]);
    }

    private static RouteEndpoint[] CreateEndpoints()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton(new ShellSettings { Name = ShellSettings.DefaultShellName });
        builder.Services.AddSingleton(Mock.Of<IShellHost>());
        builder.Services.AddSingleton(Mock.Of<IShellSettingsManager>());
        builder.Services.AddSingleton(Mock.Of<IShellFeaturesManager>());
        builder.Services.AddSingleton(Mock.Of<IShellRemovalManager>());
        builder.Services.AddSingleton(new EphemeralDataProtectionProvider());
        builder.Services.AddSingleton(Mock.Of<IClock>());
        builder.Services.AddSingleton<IEnumerable<DatabaseProvider>>([]);
        builder.Services.AddSingleton(Options.Create(new TenantsOptions()));
        builder.Services.AddSingleton(Mock.Of<ITenantValidator>());
        builder.Services.AddSingleton<TenantDatabasePatternResolver>(_ => throw new NotSupportedException());
        builder.Services.AddSingleton(Mock.Of<IStringLocalizer<global::OrchardCore.Tenants.Controllers.TenantApiController>>());
        builder.Services.AddSingleton(NullLogger<global::OrchardCore.Tenants.Controllers.TenantApiController>.Instance);

        builder.Services.AddSingleton<FeatureService>(_ => throw new NotSupportedException());
        builder.Services.AddSingleton(Mock.Of<IStringLocalizer<global::OrchardCore.Features.Permissions>>());

        builder.Services.AddSingleton(Mock.Of<IRecipeExecutor>());
        builder.Services.AddSingleton(Mock.Of<IRecipeHarvester>());
        builder.Services.AddSingleton(Mock.Of<IRecipeEnvironmentProvider>());
        builder.Services.AddSingleton(Mock.Of<IStringLocalizer<global::OrchardCore.Recipes.Controllers.AdminController>>());
        builder.Services.AddSingleton(NullLogger<global::OrchardCore.Recipes.Controllers.AdminController>.Instance);

        builder.Services.AddSingleton(Mock.Of<IUsersAdminListFilterParser>());
        builder.Services.AddSingleton(Mock.Of<IUsersAdminListQueryService>());
        builder.Services.AddSingleton<UserManager<global::OrchardCore.Users.IUser>>(_ => throw new NotSupportedException());
        builder.Services.AddSingleton(Mock.Of<IUserService>());
        builder.Services.AddSingleton(Mock.Of<IStringLocalizer<global::OrchardCore.Users.Controllers.AdminController>>());

        builder.Services.AddSingleton<RoleManager<IRole>>(_ => throw new NotSupportedException());
        builder.Services.AddSingleton(Mock.Of<IRoleService>());
        builder.Services.AddSingleton(new PermissionCatalog([], new Dictionary<string, string>(), new Dictionary<string, string>()));
        builder.Services.AddSingleton(Mock.Of<IStringLocalizer<global::OrchardCore.Roles.Controllers.AdminController>>());

        var app = builder.Build();

        TenantManagementEndpoints.AddTenantManagementEndpoints(app);
        FeatureManagementEndpoints.AddFeatureManagementEndpoints(app);
        RecipeManagementEndpoints.AddRecipeManagementEndpoints(app);
        UserManagementEndpoints.AddUserManagementEndpoints(app);
        RoleManagementEndpoints.AddRoleManagementEndpoints(app);
        ThemeManagementEndpoints.AddThemeManagementEndpoints(app);
        HomeRouteManagementEndpoints.AddHomeRouteManagementEndpoints(app);

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
    }

    private static void AssertOperation(
        IEnumerable<RouteEndpoint> endpoints,
        string route,
        string method,
        string endpointName,
        string tag,
        string[] commandGroup,
        string verb,
        string expectedRequestContentType,
        int[] expectedStatuses)
    {
        var endpoint = endpoints.FirstOrDefault(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText?.TrimStart('/'), route.TrimStart('/'), StringComparison.Ordinal)
            && endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase) == true);

        Assert.NotNull(endpoint);
        Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary));
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description));

        var tags = endpoint.Metadata.GetMetadata<ITagsMetadata>();
        Assert.NotNull(tags);
        Assert.Contains(tag, tags.Tags);

        var cli = endpoint.Metadata.GetMetadata<CliOperationMetadata>();
        Assert.NotNull(cli);
        Assert.Equal(commandGroup, cli.CommandGroup);
        Assert.Equal(verb, cli.Verb);

        var authorizationPolicy = endpoint.Metadata.GetMetadata<AuthorizationPolicy>();
        Assert.NotNull(authorizationPolicy);
        Assert.Contains(OrchardCoreConstants.AuthenticationSchemes.Api, authorizationPolicy.AuthenticationSchemes);
        Assert.Contains(
            authorizationPolicy.Requirements.OfType<global::OrchardCore.Security.PermissionRequirement>(),
            requirement => requirement.Permission == RemoteManagementPermissions.AccessRemoteManagement);

        var produces = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>().Select(metadata => metadata.StatusCode).ToHashSet();
        foreach (var statusCode in expectedStatuses)
        {
            Assert.Contains(statusCode, produces);
        }

        if (expectedRequestContentType is not null)
        {
            var accepts = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();
            Assert.NotNull(accepts);
            Assert.Contains(expectedRequestContentType, accepts.ContentTypes);
        }
    }
}
