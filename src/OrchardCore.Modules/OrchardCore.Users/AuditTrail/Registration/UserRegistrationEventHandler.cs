using Microsoft.AspNetCore.Identity;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.Registration;

public class UserRegistrationEventHandler : RegistrationFormEventsBase
{
    private readonly IAuditTrailManager _auditTrailManager;
    private readonly UserManager<IUser> _userManager;

    public UserRegistrationEventHandler(
        IAuditTrailManager auditTrailManager,
        UserManager<IUser> userManager)
    {
        _auditTrailManager = auditTrailManager;
        _userManager = userManager;
    }

    public override Task RegisteredAsync(IUser user)
        => RecordAuditTrailEventAsync(UserRegistrationAuditTrailEventConfiguration.Registered, user);

    private async Task RecordAuditTrailEventAsync(string name, IUser user)
    {
        var userName = user.UserName;

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
