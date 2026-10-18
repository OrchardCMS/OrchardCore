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
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IOptionsMonitor<EmailProviderOptions> _providerOptions;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        INotifier notifier,
        IOptionsMonitor<EmailProviderOptions> providerOptions,
        IOptionsMonitor<EmailOptions> emailOptions,
        IEmailService emailService,
        IStringLocalizerFactory stringLocalizerFactory,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        _emailOptions = emailOptions;
        _providerOptions = providerOptions;
        _emailService = emailService;
        _stringLocalizerFactory = stringLocalizerFactory;
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

        var model = new EmailTestViewModel()
        {
            Provider = _emailOptions.CurrentValue.DefaultProviderName,
        };

        PopulateModel(model);

        return View(model);
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

        PopulateModel(model);

        return View(model);
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

    private void PopulateModel(EmailTestViewModel model)
    {
        model.Providers = _providerOptions.CurrentValue.Providers
            .Where(entry => entry.Value.IsEnabled)
            .Select(entry => new SelectListItem(_stringLocalizerFactory.Localize(entry.Value.DisplayName), entry.Key))
            .OrderBy(x => x.Text)
            .ToArray();
    }
}
