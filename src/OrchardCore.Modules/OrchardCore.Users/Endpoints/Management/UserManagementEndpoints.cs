using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Endpoints.Management;

internal static class UserManagementEndpoints
{
    private const string RoutePrefix = "api/users";

    public static IEndpointRouteBuilder AddUserManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGet(RoutePrefix, ListAsync)
            .WithName("ApiListUsers")
            .WithSummary("Lists users.")
            .WithDescription("Returns users the caller can list, including role membership and account state without exposing secrets.")
            .WithCliCommand(new CliOperationMetadata(["users"], "list")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items.userId", "Id"),
                    new CliTableColumnMetadata("items.userName", "User name"),
                    new CliTableColumnMetadata("items.email", "Email"),
                    new CliTableColumnMetadata("items.isEnabled", "Enabled"),
                },
            })
            .Produces<UserListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementGet(RoutePrefix + "/{userId}", GetAsync)
            .WithName("ApiGetUser")
            .WithSummary("Gets a user.")
            .WithDescription("Returns a single user with role membership and account state without exposing secrets.")
            .WithCliCommand(new CliOperationMetadata(["users"], "show")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
            })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix, CreateAsync)
            .WithName("ApiCreateUser")
            .WithSummary("Creates a user.")
            .WithDescription("Creates a user by using the existing user service and authorized role assignment rules.")
            .WithCliCommand(new CliOperationMetadata(["users"], "create")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Options,
                SecretProperties = { "password" },
            })
            .Accepts<UserCreateRequest>("application/json")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPut(RoutePrefix + "/{userId}", UpdateAsync)
            .WithName("ApiUpdateUser")
            .WithSummary("Updates a user.")
            .WithDescription("Updates a user by using the existing identity and user services, including authorized role membership changes.")
            .WithCliCommand(new CliOperationMetadata(["users"], "update")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
                InputMode = CliInputMode.Options,
                SecretProperties = { "password" },
            })
            .Accepts<UserUpdateRequest>("application/json")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix + "/{userId}:disable", DisableAsync)
            .WithName("ApiDisableUser")
            .WithSummary("Disables a user.")
            .WithDescription("Disables a user account by using the existing user service safeguards.")
            .WithCliCommand(new CliOperationMetadata(["users"], "disable")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
                RequiresConfirmation = true,
            })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementDelete(RoutePrefix + "/{userId}", DeleteAsync)
            .WithName("ApiDeleteUser")
            .WithSummary("Deletes a user.")
            .WithDescription("Deletes a user by using the existing identity manager and resource-based permissions.")
            .WithCliCommand(new CliOperationMetadata(["users"], "delete")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
                RequiresConfirmation = true,
            })
            .Produces<UserOperationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    internal static async Task<IResult> ListAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IUsersAdminListFilterParser filterParser,
        IUsersAdminListQueryService usersAdminListQueryService,
        [AsParameters] UserListRequest request)
    {
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;

        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.ListUsers, new User()))
        {
            return httpContext.ApiForbidProblem();
        }

        var options = new UserIndexOptions { FilterResult = filterParser.Parse(BuildFilterText(request)) };
        options.FilterResult.MapTo(options);

        var users = await usersAdminListQueryService.QueryAsync(options, NoopUpdateModel.Instance);
        var count = await users.CountAsync();
        var results = await users.Skip(skip).Take(take).ListAsync();

        return TypedResults.Ok(new UserListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = count,
            Items = results.Select(ToResponse).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, IAuthorizationService authorizationService, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["User not found."]);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.ViewUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        return TypedResults.Ok(ToResponse(user));
    }

    internal static async Task<IResult> CreateAsync(HttpContext httpContext, UserCreateRequest request, UserManager<IUser> userManager, IUserService userService, IAuthorizationService authorizationService, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await userManager.FindByNameAsync(request.UserName) is User existingUser)
        {
            if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, existingUser))
            {
                return httpContext.ApiForbidProblem();
            }

            if (await MatchesCreateRequestAsync(existingUser, request, userManager))
            {
                return TypedResults.Ok(ToResponse(existingUser));
            }

            return TypedResults.Problem(
                title: "Conflict",
                detail: $"User '{request.UserName}' already exists with different settings.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = request.EmailConfirmed,
            IsEnabled = request.IsEnabled,
        };

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        user.RoleNames = await GetAuthorizedRoleNamesAsync(httpContext, serviceProvider, authorizationService, request.RoleNames ?? [], modelState);

        if (!modelState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        await userService.CreateUserAsync(user, request.Password, modelState.AddModelError);
        if (!modelState.IsValid)
        {
            if (await userManager.FindByNameAsync(request.UserName) is User concurrentUser &&
                await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, concurrentUser) &&
                await MatchesCreateRequestAsync(concurrentUser, request, userManager))
            {
                return TypedResults.Ok(ToResponse(concurrentUser));
            }

            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        return TypedResults.Created($"/{RoutePrefix}/{Uri.EscapeDataString(user.UserId)}", ToResponse(user));
    }

    internal static Task<bool> MatchesCreateRequestAsync(
        User existingUser,
        UserCreateRequest request,
        UserManager<IUser> userManager)
    {
        var existingRoles = existingUser.RoleNames ?? [];
        var requestedRoles = (request.RoleNames ?? [])
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var passwordMatches = PasswordMatches(existingUser, request.Password, userManager);

        return Task.FromResult(string.Equals(existingUser.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && string.Equals(existingUser.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal)
            && existingUser.EmailConfirmed == request.EmailConfirmed
            && existingUser.IsEnabled == request.IsEnabled
            && existingRoles.Order(StringComparer.OrdinalIgnoreCase)
                .SequenceEqual(requestedRoles.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase)
            && passwordMatches);
    }

    internal static async Task<IResult> UpdateAsync(HttpContext httpContext, string userId, UserUpdateRequest request, UserManager<IUser> userManager, IUserService userService, IAuthorizationService authorizationService, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return TypedResults.Problem(title: "Not found", detail: "User not found.", statusCode: StatusCodes.Status404NotFound);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        user.UserName = request.UserName ?? user.UserName;
        user.Email = request.Email ?? user.Email;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        if (request.EmailConfirmed.HasValue)
        {
            user.EmailConfirmed = request.EmailConfirmed.Value;
        }

        var modelState = new ModelStateDictionary();
        if (request.RoleNames is not null)
        {
            var authorizedRoleNames = await GetAuthorizedRoleNamesAsync(httpContext, serviceProvider, authorizationService, request.RoleNames, modelState);
            if (modelState.IsValid)
            {
                await UpdateUserRolesAsync(serviceProvider, userManager, user, authorizedRoleNames, httpContext.User, authorizationService);
            }
        }

        if (!modelState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                modelState.AddModelError(string.Empty, error.Description);
            }

            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        if (!string.IsNullOrWhiteSpace(request.Password) && !PasswordMatches(user, request.Password, userManager))
        {
            await userService.ResetPasswordAsync(user.UserName, await userManager.GeneratePasswordResetTokenAsync(user), request.Password, modelState.AddModelError);
        }

        if (request.IsEnabled.HasValue && request.IsEnabled.Value != user.IsEnabled)
        {
            var changed = request.IsEnabled.Value
                ? await userService.EnableAsync(user)
                : await userService.DisableAsync(user);

            if (!changed)
            {
                modelState.AddModelError(string.Empty, request.IsEnabled.Value ? "Could not enable the user." : "Could not disable the user.");
            }
        }

        if (!modelState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        return TypedResults.Ok(ToResponse(user));
    }

    private static bool PasswordMatches(User user, string password, UserManager<IUser> userManager)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return string.IsNullOrEmpty(user.PasswordHash);
        }

        return !string.IsNullOrEmpty(user.PasswordHash) &&
            userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
    }

    internal static async Task<IResult> DisableAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, IUserService userService, IAuthorizationService authorizationService, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["User not found."]);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        if (!await userService.DisableAsync(user))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Could not disable the user."]);
        }

        return TypedResults.Ok(ToResponse(user));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, IAuthorizationService authorizationService, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return TypedResults.NoContent();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.DeleteUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return TypedResults.Problem(
                title: localizer["Could not delete the user."],
                detail: string.Join(", ", result.Errors.Select(error => error.Description)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(new UserOperationResponse
        {
            UserId = userId,
            Action = "delete",
        });
    }

    internal static UserResponse ToResponse(User user)
        => new()
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            IsEnabled = user.IsEnabled,
            IsLockoutEnabled = user.IsLockoutEnabled,
            LockoutEndUtc = user.LockoutEndUtc,
            AccessFailedCount = user.AccessFailedCount,
            RoleNames = user.RoleNames?.OrderBy(role => role, StringComparer.OrdinalIgnoreCase).ToArray() ?? [],
        };

    private static async Task<string[]> GetAuthorizedRoleNamesAsync(HttpContext httpContext, IServiceProvider serviceProvider, IAuthorizationService authorizationService, IEnumerable<string> requestedRoleNames, ModelStateDictionary modelState)
    {
        var roleService = serviceProvider.GetService<IRoleService>();
        if (roleService is null)
        {
            return [];
        }

        var requested = requestedRoleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (requested.Length == 0)
        {
            return [];
        }

        var assignableRoles = await roleService.GetAssignableRolesAsync();
        var byName = assignableRoles.ToDictionary(role => role.RoleName, StringComparer.OrdinalIgnoreCase);
        var authorizedRoleNames = new List<string>();

        foreach (var requestedRoleName in requested)
        {
            if (!byName.TryGetValue(requestedRoleName, out var role))
            {
                modelState.AddModelError(nameof(UserCreateRequest.RoleNames), $"Role '{requestedRoleName}' cannot be assigned.");
                continue;
            }

            if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.AssignRoleToUsers, role))
            {
                modelState.AddModelError(nameof(UserCreateRequest.RoleNames), $"You do not have permission to assign role '{requestedRoleName}'.");
                continue;
            }

            authorizedRoleNames.Add(role.RoleName);
        }

        return authorizedRoleNames.ToArray();
    }

    internal static async Task UpdateUserRolesAsync(
        IServiceProvider serviceProvider,
        UserManager<IUser> userManager,
        User user,
        IReadOnlyCollection<string> selectedRoleNames,
        ClaimsPrincipal currentUser,
        IAuthorizationService authorizationService)
    {
        var userRoleStore = serviceProvider.GetService<IUserRoleStore<IUser>>();
        if (userRoleStore is null)
        {
            return;
        }

        var roleService = serviceProvider.GetService<IRoleService>();
        var currentUserRoleNames = await userRoleStore.GetRolesAsync(user, default);
        var rolesToRemove = currentUserRoleNames.Where(role => !selectedRoleNames.Contains(role, StringComparer.OrdinalIgnoreCase)).ToArray();
        var assignableRoles = roleService is null
            ? new Dictionary<string, IRole>(StringComparer.OrdinalIgnoreCase)
            : (await roleService.GetAssignableRolesAsync()).ToDictionary(role => role.RoleName, StringComparer.OrdinalIgnoreCase);
        var removedRole = false;

        foreach (var roleName in rolesToRemove)
        {
            if (!assignableRoles.TryGetValue(roleName, out var role) ||
                !await authorizationService.AuthorizeAsync(currentUser, UsersPermissions.AssignRoleToUsers, role))
            {
                continue;
            }

            var isAdminRole = await roleService.IsAdminRoleAsync(roleName);

            if (isAdminRole)
            {
                var enabledAdminUsers = (await userManager.GetUsersInRoleAsync(roleName))
                    .Cast<User>()
                    .Where(candidate => candidate.IsEnabled)
                    .ToArray();

                if (enabledAdminUsers.Length == 1 && enabledAdminUsers[0].UserId == user.UserId)
                {
                    continue;
                }
            }

            await userRoleStore.RemoveFromRoleAsync(user, userManager.NormalizeName(roleName), default);
            removedRole = true;
        }

        if (removedRole)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        foreach (var role in selectedRoleNames)
        {
            var normalizedName = userManager.NormalizeName(role);
            if (!await userRoleStore.IsInRoleAsync(user, normalizedName, default))
            {
                await userRoleStore.AddToRoleAsync(user, normalizedName, default);
            }
        }

        user.RoleNames = (await userRoleStore.GetRolesAsync(user, default)).ToArray();
    }

    private static string BuildFilterText(UserListRequest request)
    {
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            filters.Add(EscapeFilterValue(request.Search));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            filters.Add($"email:{EscapeFilterValue(request.Email)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            filters.Add($"role:{EscapeFilterValue(request.Role)}");
        }

        if (request.Enabled.HasValue)
        {
            filters.Add($"status:{(request.Enabled.Value ? nameof(UsersFilter.Enabled) : nameof(UsersFilter.Disabled))}");
        }

        filters.Add($"sort:{request.Sort ?? nameof(UsersOrder.Name)}");

        return string.Join(' ', filters);
    }

    private static string EscapeFilterValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Contains(' ') || value.Contains('"')
            ? '"' + value.Replace("\"", "\\\"", StringComparison.Ordinal) + '"'
            : value;
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

    internal sealed class UserListRequest
    {
        public int? Skip { get; init; }
        public int? Take { get; init; }
        public string Search { get; init; }
        public string Email { get; init; }
        public string Role { get; init; }
        public bool? Enabled { get; init; }
        public string Sort { get; init; }
    }

    internal sealed class UserCreateRequest
    {
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; }
        public string PhoneNumber { get; init; }
        public string Password { get; init; }
        public bool EmailConfirmed { get; init; }
        public bool IsEnabled { get; init; } = true;
        public string[] RoleNames { get; init; } = [];
    }

    internal sealed class UserUpdateRequest
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string PhoneNumber { get; init; }
        public string Password { get; init; }
        public bool? EmailConfirmed { get; init; }
        public bool? IsEnabled { get; init; }
        public string[] RoleNames { get; init; }
    }

    internal sealed class UserListResponse
    {
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public UserResponse[] Items { get; init; } = [];
    }

    internal sealed class UserResponse
    {
        public string UserId { get; init; } = string.Empty;
        public string UserName { get; init; }
        public string Email { get; init; }
        public string PhoneNumber { get; init; }
        public bool EmailConfirmed { get; init; }
        public bool IsEnabled { get; init; }
        public bool IsLockoutEnabled { get; init; }
        public DateTimeOffset? LockoutEndUtc { get; init; }
        public int AccessFailedCount { get; init; }
        public string[] RoleNames { get; init; } = [];
    }

    internal sealed class UserOperationResponse
    {
        public string UserId { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
    }

    private sealed class NoopUpdateModel : IUpdateModel
    {
        public static NoopUpdateModel Instance { get; } = new();

        public ModelStateDictionary ModelState { get; } = new();

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class => Task.FromResult(false);
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class => Task.FromResult(false);
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class => Task.FromResult(false);
        public bool TryValidateModel(object model) => true;
        public bool TryValidateModel(object model, string prefix) => true;
    }
}

internal static class UserManagementApiEndpointConventions
{
    public const string CapabilityName = "users";
    public const string TagName = "Users";

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
