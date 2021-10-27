using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.ResetPassword
{
    public class UserResetPasswordEventHandler : IPasswordRecoveryFormEvents
    {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IServiceProvider _serviceProvider;
        private UserManager<IUser> _userManager;

        public UserResetPasswordEventHandler(
            IAuditTrailManager auditTrailManager,
            IServiceProvider serviceProvider)
        {
            _auditTrailManager = auditTrailManager;
            _serviceProvider = serviceProvider;
        }

        public Task PasswordRecoveredAsync(PasswordRecoveryContext context)
            => RecordAuditTrailEventAsync(UserResetPasswordAuditTrailEventConfiguration.PasswordRecovered, context.User);

        public Task PasswordResetAsync(PasswordRecoveryContext context)
            => RecordAuditTrailEventAsync(UserResetPasswordAuditTrailEventConfiguration.PasswordReset, context.User);

        #region Unused events

        public Task RecoveringPasswordAsync(Action<string, string> reportError) => Task.CompletedTask;

        public Task ResettingPasswordAsync(Action<string, string> reportError) => Task.CompletedTask;

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
                    UserResetPasswordAuditTrailEventConfiguration.User,
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
