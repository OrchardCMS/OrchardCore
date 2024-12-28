using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Handlers;

internal sealed class EmailConfirmationRegistrationFormEvents : RegistrationFormEventsBase
{
    private readonly RegistrationOptions _registrationOptions;
    private readonly UserManager<IUser> _userManager;
    private readonly UserEmailService _userEmailConfirmationService;

    public EmailConfirmationRegistrationFormEvents(
        IOptions<RegistrationOptions> registrationOptions,
        UserManager<IUser> userManager,
        UserEmailService userEmailConfirmationService)
    {
        _registrationOptions = registrationOptions.Value;
        _userManager = userManager;
        _userEmailConfirmationService = userEmailConfirmationService;
    }

    public override async Task RegisteringAsync(UserRegisteringContext context)
    {
        if (!_registrationOptions.UsersMustValidateEmail || await _userManager.IsEmailConfirmedAsync(context.User))
        {
            return;
        }

        await _userEmailConfirmationService.SendEmailConfirmationAsync(context.User);
    }
}
