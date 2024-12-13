using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail.ResetPassword;

public sealed class UserResetPasswordEventHandler : PasswordRecoveryFormEvents
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

    public override Task PasswordRecoveredAsync(PasswordRecoveryContext context)
        => RecordAuditTrailEventAsync(UserResetPasswordAuditTrailEventConfiguration.PasswordRecovered, context.User);

    public override Task PasswordResetAsync(PasswordRecoveryContext context)
        => RecordAuditTrailEventAsync(UserResetPasswordAuditTrailEventConfiguration.PasswordReset, context.User);

    private async Task RecordAuditTrailEventAsync(string name, IUser user)
    {
        var userName = user.UserName;

        var userEvent = new AuditTrailUserEvent
        {
            UserName = userName,
        };

        if (user is User u)
        {
            userEvent.UserId = u.UserId;
        }

        if (string.IsNullOrEmpty(userEvent.UserId))
        {
            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();
            userEvent.UserId = await _userManager.GetUserIdAsync(user);
        }

        await _auditTrailManager.RecordEventAsync(
            new AuditTrailContext<AuditTrailUserEvent>
            (
                name,
                UserResetPasswordAuditTrailEventConfiguration.User,
                userEvent.UserId,
                userEvent.UserId,
                userName,
                userEvent
            ));
    }
}
