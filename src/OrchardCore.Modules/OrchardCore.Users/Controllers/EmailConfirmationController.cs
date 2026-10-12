using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RateLimits;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

public sealed class EmailConfirmationController : Controller
{
    private const string ResendCooldownCacheKeyPrefix = "EmailConfirmationResend:";

    private static readonly TimeSpan s_resendCooldown = TimeSpan.FromMinutes(1);

    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
    private readonly UserEmailService _userEmailService;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger _logger;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public EmailConfirmationController(
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IEnumerable<IUserEventHandler> userEventHandlers,
        UserEmailService userEmailService,
        IDistributedCache distributedCache,
        ILogger<EmailConfirmationController> logger,
        IHtmlLocalizer<EmailConfirmationController> htmlLocalizer,
        IStringLocalizer<EmailConfirmationController> stringLocalizer)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _notifier = notifier;
        _userEventHandlers = userEventHandlers;
        _userEmailService = userEmailService;
        _distributedCache = distributedCache;
        _logger = logger;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            await HttpContext.SignOutAsync(UserConstants.EmailConfirmationAuthenticationScheme);

            var userContext = new UserConfirmContext(user) { ConfirmationType = UserConfirmationType.Email };
            await _userEventHandlers.InvokeAsync((handler, context) => handler.ConfirmedAsync(userContext), userContext, _logger);

            return View();
        }

        return NotFound();
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailSent(string returnUrl = null)
    {
        var user = await GetEmailConfirmationUserAsync();

        return View(new ConfirmEmailSentViewModel
        {
            ReturnUrl = returnUrl,
            CanResendEmailConfirmation = user != null && !await _userManager.IsEmailConfirmedAsync(user),
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [RateLimitGroup(UserRateLimiterPolicyNames.EmailConfirmation)]
    public async Task<IActionResult> ResendEmailConfirmation(string returnUrl = null)
    {
        var user = await GetEmailConfirmationUserAsync();

        if (user == null)
        {
            return NotFound();
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            await HttpContext.SignOutAsync(UserConstants.EmailConfirmationAuthenticationScheme);
            await _notifier.WarningAsync(H["Your email is already confirmed."]);

            return RedirectToAction(nameof(ConfirmEmailSent), new { returnUrl });
        }

        // The per-IP rate limit only applies when the Rate Limits feature is enabled,
        // so this cooldown always limits how often a user can request a new email.
        var cooldownKey = ResendCooldownCacheKeyPrefix + await _userManager.GetUserIdAsync(user);

        if (await _distributedCache.GetAsync(cooldownKey, HttpContext.RequestAborted) != null)
        {
            await _notifier.WarningAsync(H["A confirmation email was sent recently. Please wait a minute before requesting another one."]);

            return RedirectToAction(nameof(ConfirmEmailSent), new { returnUrl });
        }

        // Start the cooldown before sending, so that it also applies while the email is being sent or when sending fails.
        await _distributedCache.SetAsync(cooldownKey, [1], new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = s_resendCooldown,
        }, HttpContext.RequestAborted);

        if (await _userEmailService.SendEmailConfirmationAsync(user))
        {
            await _notifier.SuccessAsync(H["Verification email sent."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["The verification email could not be sent."]);
        }

        return RedirectToAction(nameof(ConfirmEmailSent), new { returnUrl });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendVerificationEmail(string id = null, string returnUrl = null)
    {
        var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(id))
        {
            id = currentUserId;
        }

        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        // Allow users to verify their own email without the 'ManageUsers' permission.
        if (id != currentUserId && !await _authorizationService.AuthorizeAsync(User, UsersPermissions.ManageUsers))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id) as User;

        if (user == null)
        {
            return NotFound();
        }

        await _userEmailService.SendEmailConfirmationAsync(user);

        await _notifier.SuccessAsync(H["Verification email sent."]);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(AdminController.Index), typeof(AdminController).ControllerName());
    }

    private async Task<IUser> GetEmailConfirmationUserAsync()
    {
        var result = await HttpContext.AuthenticateAsync(UserConstants.EmailConfirmationAuthenticationScheme);
        var userId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return string.IsNullOrEmpty(userId)
            ? null
            : await _userManager.FindByIdAsync(userId);
    }
}
