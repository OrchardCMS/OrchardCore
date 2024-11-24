using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.Email;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

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

        if (settings.UsersMustValidateEmail && !await _userManager.IsEmailConfirmedAsync(context.User))
        {
            await _userEmailConfirmationService.SendEmailConfirmationAsync(context.User);
        }
    }
}
