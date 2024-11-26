using Microsoft.Extensions.Options;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class UserModerationRegistrationFormEvents : RegistrationFormEventsBase
{
    private readonly RegistrationOptions _registrationOptions;

    public UserModerationRegistrationFormEvents(IOptions<RegistrationOptions> registrationOptions)
    {
        _registrationOptions = registrationOptions.Value;
    }

    public override Task RegisteringAsync(UserRegisteringContext context)
    {
        if (context.User is User user &&
            !(_registrationOptions.UsersAreModerated && !user.IsEnabled))
        {
            context.CancelSignIn = true;
        }

        return Task.CompletedTask;
    }
}
