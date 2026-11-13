using OrchardCore.Users.AuditTrail.Handlers;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class UserEventHandlerTests
{
    [Theory]
    [InlineData(nameof(User.PasswordHash))]
    [InlineData(nameof(User.ResetToken))]
    [InlineData(nameof(User.UserTokens))]
    [InlineData(nameof(User.Passkeys))]
    public void BannedProperties_SensitiveUserProperty_CannotBeStoredInSnapshot(string propertyName)
    {
        Assert.Contains(propertyName, UserEventHandler.BannedProperties);
    }
}
