using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.RemoteManagement;
using OrchardCore.Roles.Controllers;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Endpoints.Management;

internal static class RoleManagementEndpoints
{
    private const string RoutePrefix = "api/roles";

    public static IEndpointRouteBuilder AddRoleManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGet(RoutePrefix, ListAsync)
            .WithName("ApiListRoles")
            .WithSummary("Lists roles.")
            .WithDescription("Returns roles with system-role information and assigned/effective permissions.")
            .WithCliCommand(new CliOperationMetadata(["roles"], "list")
            {
                Capability = RoleManagementApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items.roleName", "Role"),
                    new CliTableColumnMetadata("items.isSystemRole", "System"),
                    new CliTableColumnMetadata("items.isAdminRole", "Admin"),
                    new CliTableColumnMetadata("items.permissionCount", "Permissions"),
                },
            })
            .Produces<RoleListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementGet(RoutePrefix + "/{roleId}", GetAsync)
            .WithName("ApiGetRole")
            .WithSummary("Gets a role.")
            .WithDescription("Returns a single role with its assigned and effective permissions.")
            .WithCliCommand(new CliOperationMetadata(["roles"], "show")
            {
                Capability = RoleManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("roleId", 0) },
            })
            .Produces<RoleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix, CreateAsync)
            .WithName("ApiCreateRole")
            .WithSummary("Creates a role.")
            .WithDescription("Creates a role and optionally assigns permissions using the existing role manager.")
            .WithCliCommand(new CliOperationMetadata(["roles"], "create")
            {
                Capability = RoleManagementApiEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
            })
            .Accepts<RoleCreateRequest>("application/json")
            .Produces<RoleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPut(RoutePrefix + "/{roleId}", UpdateAsync)
            .WithName("ApiUpdateRole")
            .WithSummary("Updates a role.")
            .WithDescription("Updates a role description and permissions by reusing the existing role manager and permission evaluation rules.")
            .WithCliCommand(new CliOperationMetadata(["roles"], "update")
            {
                Capability = RoleManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("roleId", 0) },
                InputMode = CliInputMode.Json,
            })
            .Accepts<RoleUpdateRequest>("application/json")
            .Produces<RoleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementDelete(RoutePrefix + "/{roleId}", DeleteAsync)
            .WithName("ApiDeleteRole")
            .WithSummary("Deletes a role.")
            .WithDescription("Deletes a non-system role.")
            .WithCliCommand(new CliOperationMetadata(["roles"], "delete")
            {
                Capability = RoleManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("roleId", 0) },
                RequiresConfirmation = true,
            })
            .Produces<RoleOperationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    internal static async Task<IResult> ListAsync(HttpContext httpContext, IAuthorizationService authorizationService, IRoleService roleService, PermissionCatalog permissionCatalog, [AsParameters] RoleListRequest request)
    {
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;

        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, RolesPermissions.ViewRoles))
        {
            return httpContext.ApiForbidProblem();
        }

        var roles = await roleService.GetRolesAsync();
        var filtered = roles
            .OfType<Role>()
            .Where(role => string.IsNullOrWhiteSpace(request.Search)
                || role.RoleName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || (role.RoleDescription?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(role => role.RoleName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var items = new List<RoleResponse>();
        foreach (var role in filtered.Skip(skip).Take(take))
        {
            items.Add(await ToResponseAsync(role, authorizationService, roleService, permissionCatalog));
        }

        return TypedResults.Ok(new RoleListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = filtered.Length,
            Items = items.ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext httpContext, string roleId, IAuthorizationService authorizationService, RoleManager<IRole> roleManager, IRoleService roleService, PermissionCatalog permissionCatalog, IStringLocalizer<AdminController> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, RolesPermissions.ViewRoles))
        {
            return httpContext.ApiForbidProblem();
        }

        if (await roleManager.FindByIdAsync(roleId) is not Role role)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Role not found."]);
        }

        return TypedResults.Ok(await ToResponseAsync(role, authorizationService, roleService, permissionCatalog));
    }

    internal static async Task<IResult> CreateAsync(HttpContext httpContext, RoleCreateRequest request, IAuthorizationService authorizationService, RoleManager<IRole> roleManager, IRoleService roleService, PermissionCatalog permissionCatalog, IStringLocalizer<AdminController> localizer)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await authorizationService.AuthorizeAsync(httpContext.User, RolesPermissions.ManageRoles))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var roleName = request.RoleName?.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            modelState.AddModelError(nameof(RoleCreateRequest.RoleName), localizer["Role name is required."]);
        }
        else if (roleName.Contains('/', StringComparison.Ordinal))
        {
            modelState.AddModelError(nameof(RoleCreateRequest.RoleName), localizer["Invalid role name."]);
        }
        var permissionNames = ValidatePermissions(request.PermissionNames ?? [], permissionCatalog, modelState);
        if (!modelState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        if (await roleManager.FindByNameAsync(roleName) is Role existing)
        {
            var assignedPermissions = existing.RoleClaims
                .Where(claim => claim.ClaimType == Permission.ClaimType)
                .Select(claim => claim.ClaimValue)
                .ToHashSet(StringComparer.Ordinal);

            if (string.Equals(existing.RoleDescription, request.RoleDescription, StringComparison.Ordinal)
                && assignedPermissions.SetEquals(permissionNames))
            {
                return TypedResults.Created($"/{RoutePrefix}/{Uri.EscapeDataString(existing.RoleName)}", await ToResponseAsync(existing, authorizationService, roleService, permissionCatalog));
            }

            return TypedResults.Problem(
                detail: localizer["A role named '{0}' already exists with a different definition.", roleName],
                statusCode: StatusCodes.Status409Conflict);
        }

        var role = new Role
        {
            RoleName = roleName,
            RoleDescription = request.RoleDescription,
            RoleClaims = permissionNames.Select(RoleClaim.Create).ToList(),
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(string.Empty, error.Description);
            }

            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        return TypedResults.Created($"/{RoutePrefix}/{Uri.EscapeDataString(role.RoleName)}", await ToResponseAsync(role, authorizationService, roleService, permissionCatalog));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext httpContext, string roleId, RoleUpdateRequest request, IAuthorizationService authorizationService, RoleManager<IRole> roleManager, IRoleService roleService, PermissionCatalog permissionCatalog)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await authorizationService.AuthorizeAsync(httpContext.User, RolesPermissions.ManageRoles))
        {
            return httpContext.ApiForbidProblem();
        }

        if (await roleManager.FindByIdAsync(roleId) is not Role role)
        {
            return TypedResults.Problem(title: "Not found", detail: "Role not found.", statusCode: StatusCodes.Status404NotFound);
        }

        role.RoleDescription = request.RoleDescription ?? role.RoleDescription;

        var modelState = new ModelStateDictionary();
        if (request.PermissionNames is not null && !await roleService.IsAdminRoleAsync(role.RoleName))
        {
            var permissionNames = ValidatePermissions(request.PermissionNames, permissionCatalog, modelState);
            if (!modelState.IsValid)
            {
                return httpContext.ApiValidationProblem(modelState: modelState);
            }

            role.RoleClaims.RemoveAll(claim => claim.ClaimType == Permission.ClaimType);
            role.RoleClaims.AddRange(permissionNames.Select(RoleClaim.Create));
        }

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(string.Empty, error.Description);
            }

            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        return TypedResults.Ok(await ToResponseAsync(role, authorizationService, roleService, permissionCatalog));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext httpContext, string roleId, IAuthorizationService authorizationService, RoleManager<IRole> roleManager, IRoleService roleService, IStringLocalizer<AdminController> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, RolesPermissions.ManageRoles))
        {
            return httpContext.ApiForbidProblem();
        }

        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            return TypedResults.Ok(new RoleOperationResponse
            {
                RoleId = roleId,
                Action = "delete",
            });
        }

        if (await roleService.IsSystemRoleAsync(role.RoleName))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["System roles cannot be deleted."]);
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return TypedResults.Problem(
                title: localizer["Could not delete this role."],
                detail: string.Join(", ", result.Errors.Select(error => error.Description)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(new RoleOperationResponse
        {
            RoleId = roleId,
            Action = "delete",
        });
    }

    internal static async Task<RoleResponse> ToResponseAsync(Role role, IAuthorizationService authorizationService, IRoleService roleService, PermissionCatalog permissionCatalog)
    {
        var isSystemRole = await roleService.IsSystemRoleAsync(role.RoleName);
        var isAdminRole = isSystemRole && await roleService.IsAdminRoleAsync(role.RoleName);
        var assignedPermissions = role.RoleClaims.Where(claim => claim.ClaimType == Permission.ClaimType).Select(claim => claim.ClaimValue).ToHashSet(StringComparer.Ordinal);
        var effectivePermissions = isAdminRole
            ? permissionCatalog.Permissions.Select(permission => permission.Name).ToHashSet(StringComparer.Ordinal)
            : await GetEffectivePermissionsAsync(role, authorizationService, permissionCatalog.Permissions);

        var permissions = permissionCatalog.Permissions
            .OrderBy(permission => permission.Name, StringComparer.OrdinalIgnoreCase)
            .Select(permission => new RolePermissionResponse
            {
                Name = permission.Name,
                Description = permission.Description,
                Category = permission.Category,
                FeatureId = permissionCatalog.GetFeatureId(permission.Name),
                FeatureName = permissionCatalog.GetFeatureName(permission.Name),
                IsSecurityCritical = permission.IsSecurityCritical,
                IsAssigned = assignedPermissions.Contains(permission.Name),
                IsEffective = effectivePermissions.Contains(permission.Name),
            })
            .ToArray();

        return new RoleResponse
        {
            RoleId = role.RoleName,
            RoleName = role.RoleName,
            RoleDescription = role.RoleDescription,
            IsSystemRole = isSystemRole,
            IsAdminRole = isAdminRole,
            PermissionCount = permissions.Count(permission => permission.IsEffective),
            Permissions = permissions,
        };
    }

    private static string[] ValidatePermissions(IEnumerable<string> requestedPermissions, PermissionCatalog permissionCatalog, ModelStateDictionary modelState)
    {
        var validNames = permissionCatalog.Permissions.Select(permission => permission.Name).ToHashSet(StringComparer.Ordinal);
        var validated = new List<string>();

        foreach (var requestedPermission in requestedPermissions.Where(permission => !string.IsNullOrWhiteSpace(permission)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!validNames.Contains(requestedPermission))
            {
                modelState.AddModelError(nameof(RoleCreateRequest.PermissionNames), $"Permission '{requestedPermission}' is not installed.");
                continue;
            }

            validated.Add(requestedPermission);
        }

        return validated.ToArray();
    }

    private static async Task<HashSet<string>> GetEffectivePermissionsAsync(Role role, IAuthorizationService authorizationService, IEnumerable<Permission> permissions)
    {
        var authenticationType = !string.Equals(role.RoleName, OrchardCoreConstants.Roles.Anonymous, StringComparison.OrdinalIgnoreCase)
            ? "FakeAuthenticationType"
            : null;

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, role.RoleName)], authenticationType);
        identity.AddClaims(role.RoleClaims.Select(claim => claim.ToClaim()));
        var principal = new ClaimsPrincipal(identity);

        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var permission in permissions)
        {
            if (await authorizationService.AuthorizeAsync(principal, permission))
            {
                result.Add(permission.Name);
            }
        }

        return result;
    }

    private static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(title: "Bad request", detail: "Skip must be zero or greater and take must be greater than zero.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > 200)
        {
            return TypedResults.Problem(title: "Bad request", detail: "Take cannot exceed 200.", statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
    }

    internal sealed class RoleListRequest
    {
        public int? Skip { get; init; }
        public int? Take { get; init; }
        public string Search { get; init; }
    }

    internal sealed class RoleCreateRequest
    {
        public string RoleName { get; init; } = string.Empty;
        public string RoleDescription { get; init; }
        public string[] PermissionNames { get; init; } = [];
    }

    internal sealed class RoleUpdateRequest
    {
        public string RoleDescription { get; init; }
        public string[] PermissionNames { get; init; }
    }

    internal sealed class RoleListResponse
    {
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public RoleResponse[] Items { get; init; } = [];
    }

    internal sealed class RoleResponse
    {
        public string RoleId { get; init; } = string.Empty;
        public string RoleName { get; init; } = string.Empty;
        public string RoleDescription { get; init; }
        public bool IsSystemRole { get; init; }
        public bool IsAdminRole { get; init; }
        public int PermissionCount { get; init; }
        public RolePermissionResponse[] Permissions { get; init; } = [];
    }

    internal sealed class RolePermissionResponse
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; }
        public string Category { get; init; }
        public string FeatureId { get; init; }
        public string FeatureName { get; init; }
        public bool IsSecurityCritical { get; init; }
        public bool IsAssigned { get; init; }
        public bool IsEffective { get; init; }
    }

    internal sealed class RoleOperationResponse
    {
        public string RoleId { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
    }
}

internal static class RoleManagementApiEndpointConventions
{
    public const string CapabilityName = "roles";
    public const string TagName = "Roles";

    public static RouteHandlerBuilder MapManagementGet(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapGet(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementPost(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPost(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementPut(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPut(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementDelete(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapDelete(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    private static Action<AuthorizationPolicyBuilder> CreateBearerPolicy()
        => static policy => policy
            .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
            .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement))
            .RequireAuthenticatedUser();
}

internal sealed class PermissionCatalog
{
    private readonly IDictionary<string, Permission> _permissions;
    private readonly IDictionary<string, string> _featureIds;
    private readonly IDictionary<string, string> _featureNames;

    public PermissionCatalog(IEnumerable<Permission> permissions, IDictionary<string, string> featureIds, IDictionary<string, string> featureNames)
    {
        _permissions = permissions.ToDictionary(permission => permission.Name, StringComparer.Ordinal);
        _featureIds = featureIds;
        _featureNames = featureNames;
    }

    public IEnumerable<Permission> Permissions => _permissions.Values;

    public string GetFeatureId(string permissionName) => _featureIds.TryGetValue(permissionName, out var featureId) ? featureId : null;
    public string GetFeatureName(string permissionName) => _featureNames.TryGetValue(permissionName, out var featureName) ? featureName : null;
}
