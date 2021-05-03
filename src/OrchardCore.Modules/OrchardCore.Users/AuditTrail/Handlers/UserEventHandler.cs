using System;
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

        public UserEventHandler(
            IAuditTrailManager auditTrailManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditTrailManager = auditTrailManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task LoggedInAsync(string userName) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.LoggedIn, userName);

        public Task LoggingInFailedAsync(string userName) =>
             RecordAuditTrailEventAsync(UserAuditTrailEventProvider.LogInFailed, userName);

        public Task PasswordRecoveredAsync() =>
            // We don't have the User in the HttpContext, the only way to get the user is by getting the value
            // of the Email from the form.
            RecordAuditTrailEventAsync(
                UserAuditTrailEventProvider.PasswordRecovered,
                _httpContextAccessor.HttpContext.Request.Form["Email"]);

        public Task PasswordResetAsync() =>
            String.IsNullOrEmpty(_httpContextAccessor.HttpContext?.User?.Identity?.Name) ?
                RecordAuditTrailEventAsync(UserAuditTrailEventProvider.PasswordReset, _httpContextAccessor.HttpContext.Request.Form["Email"]) :
                RecordAuditTrailEventAsync(UserAuditTrailEventProvider.PasswordReset, _httpContextAccessor.HttpContext.User.Identity.Name);

        public Task RegisteredAsync(IUser user) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.SignedUp, user.UserName);

        public Task DisabledAsync(UserContext context) =>
           RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Disabled, context.User);

        public Task EnabledAsync(UserContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Enabled, context.User);

        public Task CreatedAsync(UserContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Created, context.User);

        public Task DeletedAsync(UserContext context) =>
            RecordAuditTrailEventAsync(UserAuditTrailEventProvider.Deleted, context.User);

        #region Unused events

        public Task RecoveringPasswordAsync(Action<string, string> reportError) => Task.CompletedTask;

        public Task ResettingPasswordAsync(Action<string, string> reportError) => Task.CompletedTask;

        public Task LoggingInAsync(string userName, Action<string, string> reportError) => Task.CompletedTask;

        public Task RegistrationValidationAsync(Action<string, string> reportError) =>
            Task.CompletedTask;

        #endregion

        private async Task RecordAuditTrailEventAsync(string eventName, string username)
        {
            if (String.IsNullOrEmpty(username))
            {
                return;
            }

            var userManager = GetUserManagerFromHttpContext();

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                var eventData = new Dictionary<string, object>
                {
                    { "UserName", username }
                };

                await _auditTrailManager.AddAuditTrailEventAsync<UserAuditTrailEventProvider>(
                    new AuditTrailContext(eventName, username, eventData, "user", username));
            }
            else
            {
                var eventData = new Dictionary<string, object>
                {
                    { "UserId", await userManager.GetUserIdAsync(user) },
                    { "UserName", user.UserName }
                };

                await _auditTrailManager.AddAuditTrailEventAsync<UserAuditTrailEventProvider>(
                    new AuditTrailContext(eventName, user.UserName, eventData, "user", user.UserName));
            }
        }

        private async Task RecordAuditTrailEventAsync(string eventName, IUser user)
        {
            var userName = user.UserName;
            var userManager = GetUserManagerFromHttpContext();

            var eventData = new Dictionary<string, object>
            {
                { "UserId", await userManager.GetUserIdAsync(user) },
                { "UserName", userName }
            };

            await _auditTrailManager.AddAuditTrailEventAsync<UserAuditTrailEventProvider>(
                new AuditTrailContext(
                    eventName,
                    eventName == UserAuditTrailEventProvider.Created ? userName : _httpContextAccessor.GetCurrentUserName(),
                    eventData,
                    "user",
                    userName));
        }

        // Need to resolve the UserManager from the HttpContext to prevent circular dependency.
        private UserManager<IUser> GetUserManagerFromHttpContext() =>
            _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();
    }
}
