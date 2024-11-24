using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class UserModerationRegistrationFormEvents : RegistrationFormEventsBase
{
    private readonly ISiteService _siteService;

    public UserModerationRegistrationFormEvents(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task RegisteringAsync(UserRegisteringContext context)
    {
        var settings = await _siteService.GetSettingsAsync<RegistrationSettings>();

        if (context.User is not User user)
        {
            return;
        }

        if (!(settings.UsersAreModerated && !user.IsEnabled))
        {
            context.Cancel = true;
        }
    }
}
