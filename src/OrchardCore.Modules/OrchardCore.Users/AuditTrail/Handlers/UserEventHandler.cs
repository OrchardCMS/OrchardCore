using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Services;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.AuditTrail.Handlers
{
    public class UserEventHandler : ILoginFormEvent, IUserEventHandler
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
            => _auditTrailManager.RecordEventAsync(
                    new AuditTrailContext<AuditTrailUserEvent>
                    (
                        name: UserAuditTrailEventConfiguration.LogInFailed,
                        category: UserAuditTrailEventConfiguration.User,
                        correlationId: String.Empty,
                        userId: String.Empty,
                        userName: userName,
                        new AuditTrailUserEvent
                        {
                            UserId = String.Empty,
                            UserName = userName
                        }
                    ));

        public Task IsLockedOutAsync(IUser user)
            => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.LogInFailed, user);

        public Task DisabledAsync(UserContext context)
            => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Disabled, context.User, _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier), _httpContextAccessor.HttpContext.User?.Identity?.Name);

        public Task EnabledAsync(UserContext context)
             => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Enabled, context.User, _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier), _httpContextAccessor.HttpContext.User?.Identity?.Name);

        public Task CreatedAsync(UserCreateContext context)
             => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Created, context.User, _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier), _httpContextAccessor.HttpContext.User?.Identity?.Name);

        public Task UpdatedAsync(UserUpdateContext context)
             => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Updated, context.User, _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier), _httpContextAccessor.HttpContext.User?.Identity?.Name);

        public Task DeletedAsync(UserDeleteContext context)
             => RecordAuditTrailEventAsync(UserAuditTrailEventConfiguration.Deleted, context.User, _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier), _httpContextAccessor.HttpContext.User?.Identity?.Name);

        #region Unused user events

        public Task CreatingAsync(UserCreateContext context) => Task.CompletedTask;
        public Task UpdatingAsync(UserUpdateContext context) => Task.CompletedTask;
        public Task DeletingAsync(UserDeleteContext context) => Task.CompletedTask;

        #endregion

        #region Unused login events

        public Task LoggingInAsync(string userName, Action<string, string> reportError) => Task.CompletedTask;

        #endregion

        private async Task RecordAuditTrailEventAsync(string name, IUser user, string userIdActual = "", string userNameActual = "")
        {
            var userName = user.UserName;
            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();

            var userId = await _userManager.GetUserIdAsync(user);

            if (String.IsNullOrEmpty(userIdActual))
            {
                userIdActual = userId;
            }

            if (String.IsNullOrEmpty(userNameActual))
            {
                userNameActual = userName;
            }

            await _auditTrailManager.RecordEventAsync(
                new AuditTrailContext<AuditTrailUserEvent>
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
                ));
        }
    }
}
