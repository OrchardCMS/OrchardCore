using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Services;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.AuditTrail.Handlers;

public class UserEventHandler : UserEventHandlerBase, ILoginFormEvent
{
    private readonly IAuditTrailManager _auditTrailManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private UserManager<IUser> _userManager;

    public UserEventHandler(
        IAuditTrailManager auditTrailManager,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _auditTrailManager = auditTrailManager;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public Task LoggedInAsync(IUser user)
        => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.LoggedIn, user);

    public Task LoggingInFailedAsync(IUser user)
        => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.LogInFailed, user);

    public Task LoggingInFailedAsync(string userName)
    {
        var context = new AuditTrailContext<AuditTrailUserEvent>
            (
                name: UserAuditTrailEventConfiguration.LogInFailed,
                category: UserAuditTrailEventConfiguration.User,
                correlationId: string.Empty,
                userId: string.Empty,
                userName: userName,
                new AuditTrailUserEvent
                {
                    UserId = string.Empty,
                    UserName = userName
                }
            );

        return _auditTrailManager.RecordEventAsync(context);
    }

    public Task IsLockedOutAsync(IUser user)
        => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.LogInFailed, user);

    public override Task DisabledAsync(UserContext context)
        => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Disabled, context.User, GetCurrentUserId(), GetCurrentUserName());

    public override Task EnabledAsync(UserContext context)
         => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Enabled, context.User, GetCurrentUserId(), GetCurrentUserName());

    public override Task CreatedAsync(UserCreateContext context)
         => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Created, context.User, GetCurrentUserId(), GetCurrentUserName());

    public override Task UpdatedAsync(UserUpdateContext context)
         => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Updated, context.User, GetCurrentUserId(), GetCurrentUserName());

    public override Task DeletedAsync(UserDeleteContext context)
         => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Deleted, context.User, GetCurrentUserId(), GetCurrentUserName());

    private async Task RecordAuditTrailEventAsync(string name, IUser user, string userIdActual = "", string userNameActual = "")
    {
        var userName = user.UserName;
        _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();

        var userId = await _userManager.GetUserIdAsync(user);

        if (string.IsNullOrEmpty(userIdActual))
        {
            userIdActual = userId;
        }

        if (string.IsNullOrEmpty(userNameActual))
        {
            userNameActual = userName;
        }

        var context = new AuditTrailContext<AuditTrailUserEvent>
            (
                name: name,
                category: UserAuditTrailEventConfiguration.User,
                correlationId: userId,
                userId: userIdActual,
                userName: userNameActual,
                new AuditTrailUserEvent
                {
                    UserId = userId,
                    UserName = userName
                }
            );

        await _auditTrailManager.RecordEventAsync(context);
    }

    #region Unused login events
    public Task LoggingInAsync(string userName, Action<string, string> reportError)
        => Task.CompletedTask;

    public Task<IActionResult> LoggingInAsync()
        => Task.FromResult<IActionResult>(null);

    public Task<IActionResult> ValidatingLoginAsync(IUser user)
        => Task.FromResult<IActionResult>(null);
    #endregion

    private string GetCurrentUserName()
        => _httpContextAccessor.HttpContext.User?.Identity?.Name;

    private string GetCurrentUserId()
        => _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
