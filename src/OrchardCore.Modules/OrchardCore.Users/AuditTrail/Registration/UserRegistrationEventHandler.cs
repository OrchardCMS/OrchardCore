using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.Registration
{
    public class UserRegistrationEventHandler : IRegistrationFormEvents
    {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IServiceProvider _serviceProvider;
        private UserManager<IUser> _userManager;

        public UserRegistrationEventHandler(
            IAuditTrailManager auditTrailManager,
            IServiceProvider serviceProvider)
        {
            _auditTrailManager = auditTrailManager;
            _serviceProvider = serviceProvider;
        }

        public Task RegisteredAsync(IUser user) =>
            RecordAuditTrailEventAsync(UserRegistrationAuditTrailEventConfiguration.Registered, user);

        #region Unused events

        public Task RegistrationValidationAsync(Action<string, string> reportError) => Task.CompletedTask;

        #endregion

        private async Task RecordAuditTrailEventAsync(string name, IUser user)
        {
            var userName = user.UserName;
            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();

            var userId = await _userManager.GetUserIdAsync(user);

            await _auditTrailManager.RecordEventAsync(
                new AuditTrailContext<AuditTrailUserEvent>
                (
                    name,
                    UserRegistrationAuditTrailEventConfiguration.User,
                    userId,
                    userId,
                    userName,
                    new AuditTrailUserEvent
                    {
                        UserId = userId,
                        UserName = userName
                    }
                ));
        }
    }
}
