using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Controllers;

[Feature(UserConstants.Features.UserRegistration)]
public sealed class RegistrationController : Controller
{
    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly IDisplayManager<RegisterUserForm> _registerUserDisplayManager;
    private readonly RegistrationOptions _registrationOptions;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IEnumerable<ILoginFormEvent> _accountEvents;
    private readonly IUserService _userService;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public RegistrationController(
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService,
        ISiteService siteService,
        INotifier notifier,
        ILogger<RegistrationController> logger,
        IDisplayManager<RegisterUserForm> registerUserDisplayManager,
        IOptions<RegistrationOptions> registrationOptions,
        IUpdateModelAccessor updateModelAccessor,
        IEnumerable<ILoginFormEvent> accountEvents,
        IUserService userService,
        IHtmlLocalizer<RegistrationController> htmlLocalizer,
        IStringLocalizer<RegistrationController> stringLocalizer)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _siteService = siteService;
        _notifier = notifier;
        _logger = logger;
        _registerUserDisplayManager = registerUserDisplayManager;
        _registrationOptions = registrationOptions.Value;
        _updateModelAccessor = updateModelAccessor;
        _accountEvents = accountEvents;
        _userService = userService;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Register(string returnUrl = null)
    {
        var shape = await _registerUserDisplayManager.BuildEditorAsync(_updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        ViewData["ReturnUrl"] = returnUrl;

        return View(shape);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Register))]
    public async Task<IActionResult> RegisterPOST(string returnUrl = null)
    {
        var model = new RegisterUserForm();

        var shape = await _registerUserDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var iUser = await _userService.RegisterAsync(model, ModelState.AddModelError);

            // If we get a user, redirect to returnUrl.
            if (iUser is User user)
            {
                foreach (var accountEvent in _accountEvents)
                {
                    var loginResult = await accountEvent.ValidatingLoginAsync(user);

                    if (loginResult != null)
                    {
                        return loginResult;
                    }
                }

                return RedirectToLocal(returnUrl.ToUriComponents());
            }
        }

        // If we got this far, something failed. Let's redisplay form.
        return View(shape);
    }

    [AllowAnonymous]
    public IActionResult RegistrationPending(string returnUrl = null)
        => View(new { ReturnUrl = returnUrl });

    private RedirectResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("~/");
    }
}
