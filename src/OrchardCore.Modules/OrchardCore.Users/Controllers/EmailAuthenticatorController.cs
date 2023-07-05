using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Feature(UserConstants.Features.EmailAuthenticator)]
public class EmailAuthenticatorController : TwoFactorAuthenticationBaseController
{
    private readonly IUserService _userService;
    private readonly ISmtpService _smtpService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly HtmlEncoder _htmlEncoder;

    public EmailAuthenticatorController(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IOptions<TwoFactorOptions> twoFactorOptions,
        INotifier notifier,
        IDistributedCache distributedCache,
        IUserService userService,
        ISmtpService smtpService,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorAuthenticationHandlerCoordinator)
        : base(
            userManager,
            distributedCache,
            signInManager,
            twoFactorAuthenticationHandlerCoordinator,
            notifier,
            siteService,
            htmlLocalizer,
            stringLocalizer,
            twoFactorOptions)
    {
        _userService = userService;
        _smtpService = smtpService;
        _liquidTemplateManager = liquidTemplateManager;
        _htmlEncoder = htmlEncoder;
    }

    [Admin, Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Unable to load user.");
        }

        var email = await UserManager.GetEmailAsync(user);

        if (String.IsNullOrEmpty(email))
        {
            await Notifier.ErrorAsync(H["Your account does not have an email address. Please edit your profile and provide an email address."]);
        }

        return View();
    }

    [HttpPost, ValidateAntiForgeryToken, Admin, Authorize]
    public async Task<IActionResult> RequestCode()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Unable to load user.");
        }

        var email = await UserManager.GetEmailAsync(user);

        if (String.IsNullOrEmpty(email))
        {
            return RedirectToAction(nameof(Index));
        }

        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);

        var setings = (await SiteService.GetSiteSettingsAsync()).As<EmailAuthenticatorLoginSettings>();
        var message = new MailMessage()
        {
            To = email,
            Subject = await GetSubjectAsync(setings, user, code),
            Body = await GetMessageAsync(setings, user, code),
            IsHtmlBody = true,
        };

        var result = await _smtpService.SendAsync(message);

        if (result.Succeeded)
        {
            await Notifier.SuccessAsync(H["Please check your email for an authentication token."]);
        }
        else
        {
            await Notifier.ErrorAsync(H["We are unable to send you an email at this time. Please try again later."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new EnableEmailAuthenticatorViewModel();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize, Admin]
    public async Task<IActionResult> ValidateCode(EnableEmailAuthenticatorViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Unable to load user.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await UserManager.ConfirmEmailAsync(user, StripToken(model.Code));

        if (result.Succeeded)
        {
            await EnableTwoFactorAuthentication(user);

            await Notifier.SuccessAsync(H["Your email has been confirmed."]);

            return await RedirectToTwoFactorAsync(user);
        }

        await Notifier.ErrorAsync(H["Unable to confirm your email. Please try again."]);

        return View(nameof(RequestCode), model);
    }

    [IgnoreAntiforgeryToken, Produces("application/json"), AllowAnonymous]
    public async Task<IActionResult> GetCode()
    {
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = S["The email could not be sent. Please attempt to request the code at a later time."].Value,
            });
        }

        var setings = (await SiteService.GetSiteSettingsAsync()).As<EmailAuthenticatorLoginSettings>();
        var code = await UserManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

        var message = new MailMessage()
        {
            To = await UserManager.GetEmailAsync(user),
            Subject = await GetSubjectAsync(setings, user, code),
            Body = await GetMessageAsync(setings, user, code),
            IsHtmlBody = true,
        };

        var result = await _smtpService.SendAsync(message);

        return Ok(new
        {
            success = result.Succeeded,
            message = result.Succeeded ? S["A verification code has been sent via email. Kindly check your email for the code."].Value
            : S["The email could not be sent. Please attempt to request the code at a later time."].Value,
        });
    }

    private async Task<string> GetSubjectAsync(EmailAuthenticatorLoginSettings settings, IUser user, string code)
    {
        if (String.IsNullOrWhiteSpace(settings.Subject))
        {
            return EmailAuthenticatorLoginSettings.DefaultSubject;
        }

        return await GetMessageContentAsync(settings.Subject, user, code);
    }

    private async Task<string> GetMessageAsync(EmailAuthenticatorLoginSettings settings, IUser user, string code)
    {
        var body = settings.Body;

        if (String.IsNullOrWhiteSpace(body))
        {
            body = EmailAuthenticatorLoginSettings.DefaultBody;
        }

        return await GetMessageContentAsync(body, user, code);
    }

    private async Task<string> GetMessageContentAsync(string message, IUser user, string code)
    {
        var result = await _liquidTemplateManager.RenderHtmlContentAsync(message, _htmlEncoder, null,
            new Dictionary<string, FluidValue>()
            {
                ["User"] = new ObjectValue(user),
                ["Code"] = new StringValue(code),
            });

        using var writer = new System.IO.StringWriter();
        result.WriteTo(writer, _htmlEncoder);

        return writer.ToString();
    }
}
