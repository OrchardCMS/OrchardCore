using Microsoft.AspNetCore.Identity;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Handlers;

internal sealed class EmailConfirmationRegistrationFormEvents : RegistrationFormEventsBase
{
    private readonly ISiteService _siteService;
    private readonly UserManager<IUser> _userManager;
    private readonly UserEmailService _userEmailConfirmationService;

    public EmailConfirmationRegistrationFormEvents(
        ISiteService siteService,
        UserManager<IUser> userManager,
        UserEmailService userEmailConfirmationService)
    {
        _siteService = siteService;
        _userManager = userManager;
        _userEmailConfirmationService = userEmailConfirmationService;
    }

    public override async Task RegisteringAsync(UserRegisteringContext context)
    {
        var settings = await _siteService.GetSettingsAsync<LoginSettings>();

        if (!settings.UsersMustValidateEmail || await _userManager.IsEmailConfirmedAsync(context.User))
        {
            return;
        }

        await _userEmailConfirmationService.SendEmailConfirmationAsync(context.User);
    }
}
