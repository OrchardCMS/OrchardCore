using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Modules;
using OrchardCore.Users.AuditTrail.Providers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.AuditTrail.Handlers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class UserEventHandler : ILoginFormEvent, IPasswordRecoveryFormEvents, IRegistrationFormEvents, IUserEventHandler
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

        public Task LoggedInAsync(string userName) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.LoggedIn, userName);

        public Task LoggingInFailedAsync(string userName) =>
             RecordAuditTrailEventAsync(UserAuditTrailEventProvider.LogInFailed, userName);

        public Task PasswordRecoveredAsync() =>
            // We don't have the User in the HttpContext, the only way to get the user is by getting the value
            // of the Email from the form.
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.PasswordRecovered, _httpContextAccessor.HttpContext.Request.Form["Email"]);

        public Task PasswordResetAsync() =>
            String.IsNullOrEmpty(_httpContextAccessor.HttpContext?.User?.Identity?.Name) ?
                RecordAuditTrailEventAsync(UserAuditTrailEventProvider.PasswordReset, _httpContextAccessor.HttpContext.Request.Form["Email"]) :
                RecordAuditTrailEventAsync(UserAuditTrailEventProvider.PasswordReset, _httpContextAccessor.HttpContext.User?.Identity?.Name);

        public Task RegisteredAsync(IUser user) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Registered, user.UserName);

        public Task DisabledAsync(UserContext context) =>
           RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Disabled, context.User);

        public Task EnabledAsync(UserContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Enabled, context.User);

        public Task CreatedAsync(UserCreateContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Created, context.User);

        public Task UpdatedAsync(UserUpdateContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Updated, context.User);

        public Task DeletedAsync(UserDeleteContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Deleted, context.User);

        #region Unused events

        public Task CreatingAsync(UserCreateContext context) => Task.CompletedTask;

        public Task UpdatingAsync(UserUpdateContext context) => Task.CompletedTask;

        public Task DeletingAsync(UserDeleteContext context) => Task.CompletedTask;

        public Task RecoveringPasswordAsync(Action<string, string> reportError) => Task.CompletedTask;

        public Task ResettingPasswordAsync(Action<string, string> reportError) => Task.CompletedTask;

        public Task LoggingInAsync(string userName, Action<string, string> reportError) => Task.CompletedTask;

        public Task RegistrationValidationAsync(Action<string, string> reportError) => Task.CompletedTask;

        #endregion

        private async Task RecordAuditTrailEventAsync(string name, string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                return;
            }

            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await _userManager.FindByNameAsync(userName);

            var userId = String.Empty;
            if (user != null)
            {
                userId = await _userManager.GetUserIdAsync(user);
                userName = user.UserName;
            }

            var eventData = new Dictionary<string, object>
            {
                { "UserId", userId },
                { "UserName", user.UserName }
            };

            await _auditTrailManager.RecordEventAsync(new AuditTrailContext(name, "User", userId, userId, userName, eventData));
        }

        private async Task RecordAuditTrailEventAsync(string name, IUser user)
        {
            var userName = user.UserName;
            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();

            var userId = await _userManager.GetUserIdAsync(user);
            var data = new Dictionary<string, object>
            {
                { "UserId", userId },
                { "UserName", userName }
            };

            await _auditTrailManager.RecordEventAsync(
                new AuditTrailContext
                (
                    name,
                    "User",
                    userId,
                    userId == UserAuditTrailEventProvider.Created ? userId : _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                    name == UserAuditTrailEventProvider.Created ? userName : _httpContextAccessor.HttpContext.User?.Identity?.Name,
                    data
                ));
        }
    }
}
