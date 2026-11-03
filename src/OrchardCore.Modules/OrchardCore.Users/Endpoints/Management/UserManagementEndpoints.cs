using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
using OrchardCore.Settings;
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
        var routes = builder.MapManagementGroup(UserManagementApiEndpointConventions.TagName);

        routes.MapGet(RoutePrefix, ListAsync)
            .WithName("ApiListUsers")
            .WithSummary("Lists users.")
            .WithDescription("Returns users the caller can list, including role membership and account state without exposing secrets.")
            .WithCliCommand(new CliOperationMetadata(["users"], "list")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                ItemsProperty = "items",
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

        routes.MapGet(RoutePrefix + "/{userId}", GetAsync)
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

        routes.MapPost(RoutePrefix, CreateAsync)
            .WithName("ApiCreateUser")
            .WithSummary("Creates a user.")
            .WithDescription("Creates a user by using the existing user service and authorized role assignment rules.")
            .WithCliCommand(new CliOperationMetadata(["users"], "create")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                ResourceName = "User",
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

        routes.MapPut(RoutePrefix + "/{userId}", UpdateAsync)
            .WithName("ApiUpdateUser")
            .WithSummary("Updates a user.")
            .WithDescription("Updates a user by using the existing identity and user services, including authorized role membership changes.")
            .WithCliCommand(new CliOperationMetadata(["users"], "update")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                ResourceName = "User",
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

        routes.MapPost(RoutePrefix + "/{userId}:disable", DisableAsync)
            .WithName("ApiDisableUser")
            .WithSummary("Disables a user.")
            .WithDescription("Disables a user account by using the existing user service safeguards.")
            .WithCliCommand(new CliOperationMetadata(["users"], "disable")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                ResourceName = "User",
                Arguments = { new CliArgumentMetadata("userId", 0) },
                RequiresConfirmation = true,
            })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost(RoutePrefix + "/{userId}:unlock", UnlockAsync)
            .WithName("ApiUnlockUser")
            .WithSummary("Unlocks a user.")
            .WithDescription("Ends a user's lockout and resets the failed-access count.")
            .WithCliCommand(new CliOperationMetadata(["users"], "unlock")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
            })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost(RoutePrefix + "/{userId}:confirm-email", ConfirmEmailAsync)
            .WithName("ApiConfirmUserEmail")
            .WithSummary("Confirms a user's email.")
            .WithDescription("Marks the user's email as confirmed. An already confirmed email is left unchanged.")
            .WithCliCommand(new CliOperationMetadata(["users"], "confirm-email")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
            })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost(RoutePrefix + "/{userId}:send-reset", SendPasswordResetAsync)
            .WithName("ApiSendUserPasswordReset")
            .WithSummary("Sends a password reset email to a user.")
            .WithDescription("Sends the password reset email. Requires the password reset feature and setting, the email feature, and an address on the user. It returns no reset token.")
            .WithCliCommand(new CliOperationMetadata(["users"], "send-reset")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("userId", 0) },
            })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        routes.MapDelete(RoutePrefix + "/{userId}", DeleteAsync)
            .WithName("ApiDeleteUser")
            .WithSummary("Deletes a user.")
            .WithDescription("Deletes a user by using the existing identity manager and resource-based permissions.")
            .WithCliCommand(new CliOperationMetadata(["users"], "delete")
            {
                Capability = UserManagementApiEndpointConventions.CapabilityName,
                ResourceName = "User",
                Arguments = { new CliArgumentMetadata("userId", 0) },
                RequiresConfirmation = true,
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

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

        if (httpContext.ApiPagingProblem(skip, take) is { } pagingError)
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
        if (request is null)
        {
            return httpContext.ApiMissingBodyProblem();
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = request.EmailConfirmed,
            IsEnabled = request.IsEnabled,
        };

        // Authorize before the lookup so that a caller who can't create users can't probe user names.
        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        if (!string.IsNullOrEmpty(request.UserName) && await userManager.FindByNameAsync(request.UserName) is User existingUser)
        {
            return await CanRetryCreateAsync(httpContext, existingUser, request, authorizationService)
                ? TypedResults.Ok(ToResponse(existingUser))
                : UserNameNotAvailableProblem();
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
            if (!string.IsNullOrEmpty(request.UserName) && await userManager.FindByNameAsync(request.UserName) is User concurrentUser)
            {
                return await CanRetryCreateAsync(httpContext, concurrentUser, request, authorizationService)
                    ? TypedResults.Ok(ToResponse(concurrentUser))
                    : UserNameNotAvailableProblem();
            }

            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        return TypedResults.Created($"/{RoutePrefix}/{Uri.EscapeDataString(user.UserId)}", ToResponse(user));
    }

    // The password is left out so that the response never tells whether a guessed password is the user's.
    internal static bool MatchesCreateRequest(User existingUser, UserCreateRequest request)
    {
        var existingRoles = existingUser.RoleNames ?? [];
        var requestedRoles = (request.RoleNames ?? [])
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Equals(existingUser.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && string.Equals(existingUser.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal)
            && existingUser.EmailConfirmed == request.EmailConfirmed
            && existingUser.IsEnabled == request.IsEnabled
            && existingRoles.Order(StringComparer.OrdinalIgnoreCase)
                .SequenceEqual(requestedRoles.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<bool> CanRetryCreateAsync(HttpContext httpContext, User existingUser, UserCreateRequest request, IAuthorizationService authorizationService)
        => await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, existingUser)
            && MatchesCreateRequest(existingUser, request);

    // The only response for a name that is taken, whether the caller can edit that user, and whatever settings it has.
    private static ProblemHttpResult UserNameNotAvailableProblem()
        => TypedResults.Problem(
            title: "Conflict",
            detail: "The user name is not available with these settings.",
            statusCode: StatusCodes.Status409Conflict);

    internal static async Task<IResult> UpdateAsync(HttpContext httpContext, string userId, UserUpdateRequest request, UserManager<IUser> userManager, IUserService userService, IAuthorizationService authorizationService, IServiceProvider serviceProvider)
    {
        if (request is null)
        {
            return httpContext.ApiMissingBodyProblem();
        }

        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return TypedResults.Problem(title: "Not found", detail: "User not found.", statusCode: StatusCodes.Status404NotFound);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        // Report what would fail in the later steps before saving anything.
        var preflight = new ModelStateDictionary();
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            foreach (var validator in userManager.PasswordValidators)
            {
                foreach (var error in (await validator.ValidateAsync(userManager, user, request.Password)).Errors)
                {
                    preflight.AddModelError(nameof(UserUpdateRequest.Password), error.Description);
                }
            }
        }

        if (request.IsEnabled == false && user.IsEnabled && IsCurrentUser(httpContext, user))
        {
            preflight.AddModelError(nameof(UserUpdateRequest.IsEnabled), "You cannot disable your own account.");
        }
        else if (request.IsEnabled == false && user.IsEnabled && await IsSoleEnabledAdministratorAsync(serviceProvider, userManager, user))
        {
            preflight.AddModelError(nameof(UserUpdateRequest.IsEnabled), "Could not disable the user.");
        }

        if (!preflight.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: preflight);
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

        if (IsCurrentUser(httpContext, user))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["You cannot disable your own account."]);
        }

        if (!await userService.DisableAsync(user))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Could not disable the user."]);
        }

        return TypedResults.Ok(ToResponse(user));
    }

    internal static async Task<IResult> UnlockAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, IAuthorizationService authorizationService, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["User not found."]);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var result = await userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
        {
            return TypedResults.Problem(
                title: localizer["Could not unlock the user."],
                detail: string.Join(", ", result.Errors.Select(error => error.Description)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(ToResponse(user));
    }

    internal static async Task<IResult> ConfirmEmailAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, IAuthorizationService authorizationService, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["User not found."]);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        if (await userManager.IsEmailConfirmedAsync(user))
        {
            return TypedResults.Ok(ToResponse(user));
        }

        var result = await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        if (!result.Succeeded)
        {
            return TypedResults.Problem(
                title: localizer["Could not confirm the email."],
                detail: string.Join(", ", result.Errors.Select(error => error.Description)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(ToResponse(user));
    }

    internal static async Task<IResult> SendPasswordResetAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, ISiteService siteService, IAuthorizationService authorizationService, IServiceProvider serviceProvider, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["User not found."]);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.EditUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        // The link in the email only works when the reset password pages are available.
        if (!(await siteService.GetSettingsAsync<ResetPasswordSettings>()).AllowResetPassword
            || serviceProvider.GetService<UserEmailService>() is not { } userEmailService)
        {
            return TypedResults.Problem(
                title: localizer["Password reset is not available."],
                detail: localizer["Enable the password reset feature and setting and the email feature to send a password reset email."],
                statusCode: StatusCodes.Status409Conflict);
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["The user has no email address."]);
        }

        user.ResetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        if (!await userEmailService.SendPasswordResetAsync(user))
        {
            return TypedResults.Problem(
                title: localizer["Could not send the password reset email."],
                statusCode: StatusCodes.Status502BadGateway);
        }

        return TypedResults.Ok(ToResponse(user));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext httpContext, string userId, UserManager<IUser> userManager, IAuthorizationService authorizationService, IServiceProvider serviceProvider, IStringLocalizer<AdminController> localizer)
    {
        if (await userManager.FindByIdAsync(userId) is not User user)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["User not found."]);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UsersPermissions.DeleteUsers, user))
        {
            return httpContext.ApiForbidProblem();
        }

        if (IsCurrentUser(httpContext, user))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["You cannot delete your own account."]);
        }

        if (await IsSoleEnabledAdministratorAsync(serviceProvider, userManager, user))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Cannot delete the only enabled administrator."]);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return TypedResults.Problem(
                title: localizer["Could not delete the user."],
                detail: string.Join(", ", result.Errors.Select(error => error.Description)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.NoContent();
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

    private static bool IsCurrentUser(HttpContext httpContext, User user)
    {
        // User access tokens can carry the user identifier in the subject claim only.
        var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirstValue("sub");

        return string.Equals(currentUserId, user.UserId, StringComparison.OrdinalIgnoreCase);
    }

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

            if (await roleService.IsAdminRoleAsync(roleName) && await IsOnlyEnabledUserInRoleAsync(userManager, user, roleName))
            {
                continue;
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

    private static async Task<bool> IsSoleEnabledAdministratorAsync(IServiceProvider serviceProvider, UserManager<IUser> userManager, User user)
    {
        var roleService = serviceProvider.GetService<IRoleService>();
        if (roleService is null)
        {
            return false;
        }

        foreach (var roleName in user.RoleNames ?? [])
        {
            if (await roleService.IsAdminRoleAsync(roleName) && await IsOnlyEnabledUserInRoleAsync(userManager, user, roleName))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> IsOnlyEnabledUserInRoleAsync(UserManager<IUser> userManager, User user, string roleName)
    {
        var enabledUsers = (await userManager.GetUsersInRoleAsync(roleName))
            .Cast<User>()
            .Where(candidate => candidate.IsEnabled)
            .ToArray();

        return enabledUsers.Length == 1 && enabledUsers[0].UserId == user.UserId;
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
}
