using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Services;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Controllers;

public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly EmailService _emailService;
    private readonly EmailOptions _emailOptions;
    private readonly IEnumerable<IEmailProvider> _emailProviders;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        INotifier notifier,
        EmailService emailService,
        IOptions<EmailOptions> emailOptions,
        IEnumerable<IEmailProvider> emailProviders,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        _emailService = emailService;
        _emailOptions = emailOptions.Value;
        _emailProviders = emailProviders;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("Email/Test", "EmailTest")]
    public async Task<IActionResult> Test()
    {
        if (!await _authorizationService.AuthorizeAsync(User, EmailPermissions.ManageEmailSettings))
        {
            return Forbid();
        }

        return View(GetModel());
    }

    [HttpPost]
    public async Task<IActionResult> Test(EmailTestViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EmailPermissions.ManageEmailSettings))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var message = GetMessage(model);

            try
            {
                var result = await _emailService.SendAsync(message, model.Provider);

                if (result.Succeeded)
                {
                    await _notifier.SuccessAsync(H["Message sent successfully."]);

                    return RedirectToAction(nameof(Test));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Message.Value);
                }
            }
            catch (InvalidEmailProviderException)
            {
                ModelState.AddModelError(string.Empty, S["The selected provider is invalid or no longer enabled."]);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, S["Unable to send the message using the selected provider."]);
            }
        }

        return View(GetModel());
    }

    private static MailMessage GetMessage(EmailTestViewModel testSettings)
    {
        var message = new MailMessage
        {
            To = testSettings.To,
            Bcc = testSettings.Bcc,
            Cc = testSettings.Cc,
            ReplyTo = testSettings.ReplyTo,
        };

        if (!string.IsNullOrWhiteSpace(testSettings.From))
        {
            message.From = testSettings.From;
        }

        if (!string.IsNullOrWhiteSpace(testSettings.Subject))
        {
            message.Subject = testSettings.Subject;
        }

        if (!string.IsNullOrWhiteSpace(testSettings.Body))
        {
            message.TextBody = testSettings.Body;
        }

        return message;
    }

    private EmailTestViewModel GetModel() => new EmailTestViewModel()
    {
        Provider = _emailOptions.DefaultProviderName,
        Providers = _emailProviders.Select(p => new SelectListItem(p.DisplayName, p.Name)).ToList(),
    };
}
