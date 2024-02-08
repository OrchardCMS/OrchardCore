using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Controllers;

public class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly EmailProviderOptions _providerOptions;
    private readonly IEmailService _emailService;
    private readonly IEmailProviderResolver _emailProviderResolver;

    protected readonly IHtmlLocalizer H;
    protected readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        INotifier notifier,
        IOptions<EmailProviderOptions> providerOptions,
        IEmailService emailService,
        IEmailProviderResolver emailProviderResolver,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        _providerOptions = providerOptions.Value;
        _emailService = emailService;
        _emailProviderResolver = emailProviderResolver;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public async Task<IActionResult> Test()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
        {
            return Forbid();
        }

        var model = new EmailTestViewModel();

        PopulateModel(model);

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
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (InvalidEmailProviderException)
            {
                ModelState.AddModelError(string.Empty, "The selected provider is invalid or no longer enabled.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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
            ReplyTo = testSettings.ReplyTo
        };

        if (!string.IsNullOrWhiteSpace(testSettings.Sender))
        {
            message.Sender = testSettings.Sender;
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

    private async void PopulateModel(EmailTestViewModel model)
    {
        var options = new List<SelectListItem>();

        foreach (var entry in _providerOptions.Providers)
        {
            if (!entry.Value.IsEnabled)
            {
                continue;
            }

            var provider = await _emailProviderResolver.GetAsync(entry.Key);

            options.Add(new SelectListItem(provider.Name, entry.Key));
        }

        model.Providers = options.OrderBy(x => x.Text).ToArray();
    }
}
