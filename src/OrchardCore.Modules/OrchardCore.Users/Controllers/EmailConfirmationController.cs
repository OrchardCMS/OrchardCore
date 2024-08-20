using System.Security.Claims;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Controllers;

public sealed class EmailConfirmationController : Controller
{
    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;
    private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
    private readonly ILogger<EmailConfirmationController> _logger;

    public EmailConfirmationController(
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IHtmlLocalizer<EmailConfirmationController> htmlLocalizer,
        IStringLocalizer<EmailConfirmationController> stringLocalizer,
        IEnumerable<IUserEventHandler> userEventHandlers,
        ILogger<EmailConfirmationController> logger)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
        _userEventHandlers = userEventHandlers;
        _logger = logger;
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
            var userContext = new UserContext(user);
            await _userEventHandlers.InvokeAsync((handler, context) => handler.ConfirmedAsync(userContext), userContext, _logger);

            return View();
        }

        return NotFound();
    }

    [AllowAnonymous]
    public IActionResult ConfirmEmailSent(string returnUrl = null)
        => View(new { ReturnUrl = returnUrl });

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
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
        if (id != currentUserId && !await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageUsers))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id) as User;

        if (user == null)
        {
            return NotFound();
        }

        await this.SendEmailConfirmationTokenAsync(user, S["Confirm your account"]);

        await _notifier.SuccessAsync(H["Verification email sent."]);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(AdminController.Index), typeof(AdminController).ControllerName());
    }
}
