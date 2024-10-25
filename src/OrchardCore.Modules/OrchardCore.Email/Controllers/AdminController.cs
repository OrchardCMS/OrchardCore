using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Core;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Controllers;

public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly EmailOptions _emailOptions;
    private readonly EmailProviderOptions _providerOptions;
    private readonly IEmailService _emailService;
    private readonly IEmailProviderResolver _emailProviderResolver;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        INotifier notifier,
        IOptions<EmailProviderOptions> providerOptions,
        IOptions<EmailOptions> emailOptions,
        IEmailService emailService,
        IEmailProviderResolver emailProviderResolver,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        _emailOptions = emailOptions.Value;
        _providerOptions = providerOptions.Value;
        _emailService = emailService;
        _emailProviderResolver = emailProviderResolver;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("Email/Test", "EmailTest")]
    public async Task<IActionResult> Test()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
        {
            return Forbid();
        }

        var model = new EmailTestViewModel()
        {
            Provider = _emailOptions.DefaultProviderName,
        };

        await PopulateModelAsync(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Test(EmailTestViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
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
                    foreach (var errorMessage in error.Value)
                    {
                        ModelState.AddModelError(error.Key, errorMessage);
                    }
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

        await PopulateModelAsync(model);

        return View(model);
    }

    private static MailMessage GetMessage(EmailTestViewModel testSettings)
    {
        var message = new MailMessage
        {
            To = testSettings.To,
            Bcc = testSettings.Bcc,
            Cc = testSettings.Cc,
            ReplyTo = testSettings.ReplyTo
        };

        if (!string.IsNullOrWhiteSpace(testSettings.From))
        {
            message.Sender = testSettings.From;
        }

        if (!string.IsNullOrWhiteSpace(testSettings.Subject))
        {
            message.Subject = testSettings.Subject;
        }

        if (!string.IsNullOrWhiteSpace(testSettings.Body))
        {
            message.Body = testSettings.Body;
        }

        return message;
    }

    private async Task PopulateModelAsync(EmailTestViewModel model)
    {
        var options = new List<SelectListItem>();

        foreach (var entry in _providerOptions.Providers)
        {
            if (!entry.Value.IsEnabled)
            {
                continue;
            }

            var provider = await _emailProviderResolver.GetAsync(entry.Key);

            options.Add(new SelectListItem(provider.DisplayName, entry.Key));
        }

        model.Providers = options.OrderBy(x => x.Text).ToArray();
    }
}
