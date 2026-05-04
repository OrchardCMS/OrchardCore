using GraphQL;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.AuditTrail.Indexes;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Services;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using System.Text.Json.Nodes;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Users.AuditTrail.Handlers;

public class UserEventHandler : UserEventHandlerBase, ILoginFormEvent
{
    /// <summary>
    /// Properties of <see cref="User"/> which may never be stored for security reasons.
    /// </summary>
    public static readonly string[] BannedProperties =
    [
        nameof(User.PasswordHash),
        nameof(User.ResetToken),
        nameof(User.UserTokens),
    ];
    
    private readonly IAuditTrailManager _auditTrailManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISession _session;
    private readonly ISiteService _siteService;

    private UserManager<IUser> _userManager;

    public UserEventHandler(
        IAuditTrailManager auditTrailManager,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        ISession session,
        ISiteService siteService)
    {
        _auditTrailManager = auditTrailManager;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _session = session;
        _siteService = siteService;
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
                category: UserAuditTrailEventConfiguration.CategoryName,
                correlationId: string.Empty,
                userId: string.Empty,
                userName: userName,
                new AuditTrailUserEvent
                {
                    UserId = string.Empty,
                    UserName = userName,
                }
            );

        return _auditTrailManager.RecordEventAsync(context);
    }

    public Task IsLockedOutAsync(IUser user)
        => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.LogInFailed, user);

    public override Task DisabledAsync(UserContext context)
        => RecordAuditTrailUserEventAsync(UserAuditTrailEventConfiguration.Disabled, context, _httpContextAccessor);

    public override Task EnabledAsync(UserContext context)
         => RecordAuditTrailUserEventAsync(UserAuditTrailEventConfiguration.Enabled, context, _httpContextAccessor);

    public override Task CreatedAsync(UserCreateContext context)
         => RecordAuditTrailUserEventAsync(UserAuditTrailEventConfiguration.Created, context, _httpContextAccessor);

    public override Task UpdatedAsync(UserUpdateContext context)
         => RecordAuditTrailUserEventAsync(UserAuditTrailEventConfiguration.Updated, context, _httpContextAccessor);

    public override async Task DeletedAsync(UserDeleteContext context)
    {
        // Don't store snapshot in the delete user event for compliance with regulations about deleting personal data.
        await RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Deleted, context.User);

        // Delete snapshots from existing events.
        if (context.User is User { UserId: { Length: > 0 } userId })
        {
            var eventsToUpdate = await _session
                .Query<AuditTrailEvent, AuditTrailUserEventIndex>(
                    index => index.UserId == userId && index.HasUserSnapshot,
                    collection: AuditTrailEvent.Collection)
                .ListAsync();

            foreach (var eventToUpdate in eventsToUpdate)
            {
                eventToUpdate.Alter<AuditTrailUserEvent>(data => data.Snapshot = null);
                await _session.SaveAsync(eventToUpdate, collection: AuditTrailEvent.Collection);
            }
        }
    }

    public override Task ConfirmedAsync(UserConfirmContext context)
        => RecordAuditTrailUserEventAsync(UserAuditTrailEventConfiguration.Confirmed, context, _httpContextAccessor);

    private async Task RecordAuditTrailEventAsync(
        string name,
        IUser user,
        string userIdActual = "",
        string userNameActual = "",
        bool storeSnapshot = false)
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

        var userEvent = new AuditTrailUserEvent
        {
            UserName = userName,
            UserId = userId,
            Snapshot = storeSnapshot && user is User fullUser 
                ? CreateSnapshotObject(fullUser, await _siteService.GetSettingsAsync<AuditTrailUserEventSettings>())
                : null,
        };

        var context = new AuditTrailContext<AuditTrailUserEvent>
            (
                name: name,
                category: UserAuditTrailEventConfiguration.CategoryName,
                correlationId: userId,
                userId: userIdActual,
                userName: userNameActual,
                userEvent
            );

        await _auditTrailManager.RecordEventAsync(context);
    }
    private JsonObject CreateSnapshotObject(User fullUser, AuditTrailUserEventSettings settings)
    {
        var allowedProperties = settings.UserSnapshotProperties.ToList();
        
        // UserId is always included, to ensure that the account is identifiable within the system just using the data
        // in the snapshot. This is not PII, so storing it long term is not problematic.
        allowedProperties.Add(nameof(User.UserId));
        
        // Ensure that the User object only has the allowed properties and none of the banned ones.
        var dictionary = JObject
            .FromObject(fullUser)
            .Where(pair => !BannedProperties.Contains(pair.Key) && allowedProperties.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        
        // Custom user settings have to be handled separately.
        dictionary[nameof(User.Properties)] = JObject.FromObject(
            fullUser
                .Properties
                .ToObject<Dictionary<string, JsonObject>>()
                .Where(pair => allowedProperties.Contains(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value));

        return JObject.FromObject(dictionary);
    }

    #region Unused login events
    public Task LoggingInAsync(string userName, Action<string, string> reportError)
        => Task.CompletedTask;

    public Task<IActionResult> LoggingInAsync()
        => Task.FromResult<IActionResult>(null);

    public Task<IActionResult> ValidatingLoginAsync(IUser user)
        => Task.FromResult<IActionResult>(null);
    #endregion

    private Task RecordAuditTrailUserEventAsync(string name, UserContextBase context, IHttpContextAccessor accessor)
        => RecordAuditTrailEventAsync(
            name,
            context.User,
            accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
            accessor.HttpContext?.User?.Identity?.Name,
            storeSnapshot: true);
}
