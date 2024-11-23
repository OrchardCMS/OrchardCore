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
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Handlers;

internal class EmailConfirmationRegistrationFormEvents : RegistrationFormEventsBase
{
    private readonly ISiteService _siteService;
    private readonly UserManager<IUser> _userManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IDisplayHelper _displayHelper;
    private readonly HtmlEncoder _htmlEncoder;

    internal readonly IStringLocalizer S;

    public EmailConfirmationRegistrationFormEvents(
        ISiteService siteService,
        UserManager<IUser> userManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IDisplayHelper displayHelper,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<EmailConfirmationRegistrationFormEvents> stringLocalizer)
    {
        _siteService = siteService;
        _userManager = userManager;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _displayHelper = displayHelper;
        _htmlEncoder = htmlEncoder;
        S = stringLocalizer;
    }

    public override async Task RegisteringAsync(UserRegisteringContext context)
    {
        var settings = await _siteService.GetSettingsAsync<LoginSettings>();

        if (settings.UsersMustValidateEmail && !await _userManager.IsEmailConfirmedAsync(context.User))
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(context.User);

            var callbackUrl = _linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext, nameof(EmailConfirmationController.ConfirmEmail), typeof(EmailConfirmationController).ControllerName(),
                new
                {
                    userId = await _userManager.GetUserIdAsync(context.User),
                    code,
                });

            var email = await _userManager.GetEmailAsync(context.User);

            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            // Cancel the request to prevent the user from signing in.
            context.Cancel = true;

            await SendEmailAsync(email, S["Confirm your account"], new ConfirmEmailViewModel
            {
                User = context.User,
                ConfirmEmailUrl = callbackUrl,
            });
        }
    }

    private async Task<bool> SendEmailAsync(string email, string subject, IShape model)
    {
        var body = string.Empty;

        using (var sw = new StringWriter())
        {
            var htmlContent = await _displayHelper.ShapeExecuteAsync(model);
            htmlContent.WriteTo(sw, _htmlEncoder);
            body = sw.ToString();
        }

        var result = await _emailService.SendAsync(email, subject, body);

        return result.Succeeded;
    }
}
