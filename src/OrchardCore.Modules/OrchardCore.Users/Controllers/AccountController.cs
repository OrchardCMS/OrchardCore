using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using OrchardCore.Workflows.Helpers;
using YesSql.Services;

namespace OrchardCore.Users.Controllers;

[Authorize]
public sealed class AccountController : AccountBaseController
{
    [Obsolete("This property will be removed in v3. Instead use ExternalAuthenticationController.DefaultExternalLoginProtector")]
    public const string DefaultExternalLoginProtector = ExternalAuthenticationController.DefaultExternalLoginProtector;

    private readonly IUserService _userService;
    private readonly SignInManager<IUser> _signInManager;
    private readonly UserManager<IUser> _userManager;
    private readonly ILogger _logger;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<ILoginFormEvent> _accountEvents;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IDisplayManager<LoginForm> _loginFormDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly INotifier _notifier;
    private readonly IClock _clock;
    private readonly IDistributedCache _distributedCache;

    private static readonly JsonMergeSettings _jsonMergeSettings = new()
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
        MergeNullValueHandling = MergeNullValueHandling.Merge
    };

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AccountController(
        IUserService userService,
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ILogger<AccountController> logger,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IEnumerable<ILoginFormEvent> accountEvents,
        INotifier notifier,
        IClock clock,
        IDistributedCache distributedCache,
        IDataProtectionProvider dataProtectionProvider,
        IShellFeaturesManager shellFeaturesManager,
        IDisplayManager<LoginForm> loginFormDisplayManager,
        IUpdateModelAccessor updateModelAccessor)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
        _logger = logger;
        _siteService = siteService;
        _accountEvents = accountEvents;
        _notifier = notifier;
        _clock = clock;
        _distributedCache = distributedCache;
        _dataProtectionProvider = dataProtectionProvider;
        _shellFeaturesManager = shellFeaturesManager;
        _loginFormDisplayManager = loginFormDisplayManager;
        _updateModelAccessor = updateModelAccessor;

        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = null)
    {
        if (HttpContext.User?.Identity?.IsAuthenticated ?? false)
        {
            returnUrl = null;
        }

        // Clear the existing external cookie to ensure a clean login process.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var loginSettings = await _siteService.GetSettingsAsync<ExternalUserLoginSettings>();
        if (loginSettings.UseExternalProviderIfOnlyOneDefined)
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            if (schemes.Count() == 1)
            {
                var dataProtector = _dataProtectionProvider.CreateProtector(DefaultExternalLoginProtector)
                    .ToTimeLimitedDataProtector();

                var token = Guid.NewGuid();
                var expiration = new TimeSpan(0, 0, 5);
                var protectedToken = dataProtector.Protect(token.ToString(), _clock.UtcNow.Add(expiration));
                await _distributedCache.SetAsync(token.ToString(), token.ToByteArray(), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expiration });

                return RedirectToAction(nameof(ExternalAuthenticationController.DefaultExternalLogin), typeof(ExternalAuthenticationController).ControllerName(), new { protectedToken, returnUrl });
            }
        }

        var formShape = await _loginFormDisplayManager.BuildEditorAsync(_updateModelAccessor.ModelUpdater, false);

        CopyTempDataErrorsToModelState();

        ViewData["ReturnUrl"] = returnUrl;

        return View(formShape);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Login))]
    public async Task<IActionResult> LoginPOST(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var model = new LoginForm();

        var formShape = await _loginFormDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        var disableLocalLogin = (await _siteService.GetSettingsAsync<LoginSettings>()).DisableLocalLogin;

        if (disableLocalLogin)
        {
            ModelState.AddModelError(string.Empty, S["Local login is disabled."]);
        }
        else
        {
            await _accountEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(model.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);

            IUser user = null;
            if (ModelState.IsValid)
            {
                user = await _userService.GetUserAsync(model.UserName);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        if (!await AddConfirmEmailErrorAsync(user) && !AddUserEnabledError(user, S))
                        {
                            result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                            if (result.Succeeded)
                            {
                                _logger.LogInformation(1, "User logged in.");
                                await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

                                return await LoggedInActionResultAsync(user, returnUrl);
                            }
                        }
                    }

                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(TwoFactorAuthenticationController.LoginWithTwoFactorAuthentication),
                            typeof(TwoFactorAuthenticationController).ControllerName(),
                            new
                            {
                                returnUrl,
                                model.RememberMe
                            });
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, S["The account is locked out"]);
                        await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                        return View();
                    }
                }

                ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
            }

            if (user == null)
            {
                // Login failed unknown user.
                await _accountEvents.InvokeAsync((e, model) => e.LoggingInFailedAsync(model.UserName), model, _logger);
            }
            else
            {
                // Login failed with a known user.
                await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
            }
        }

        // If we got this far, something failed, redisplay form.
        return View(formShape);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOff(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation(4, "User logged out.");

        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public IActionResult ChangePassword(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl = null)
    {
        if (TryValidateModel(model) && ModelState.IsValid)
        {
            var user = await _userService.GetAuthenticatedUserAsync(User);
            if (await _userService.ChangePasswordAsync(user, model.CurrentPassword, model.Password, ModelState.AddModelError))
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    await _notifier.SuccessAsync(H["Your password has been changed successfully."]);

                    return this.Redirect(returnUrl, true);
                }

                return Redirect(Url.Action(nameof(ChangePasswordConfirmation)));
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ChangePasswordConfirmation()
        => View();

    public static async Task<bool> UpdateUserPropertiesAsync(UserManager<IUser> userManager, User user, UpdateUserContext context)
    {
        await userManager.AddToRolesAsync(user, context.RolesToAdd.Distinct());
        await userManager.RemoveFromRolesAsync(user, context.RolesToRemove.Distinct());

        var userNeedUpdate = false;
        if (context.PropertiesToUpdate != null)
        {
            var currentProperties = user.Properties.DeepClone();
            user.Properties.Merge(context.PropertiesToUpdate, _jsonMergeSettings);
            userNeedUpdate = !JsonNode.DeepEquals(currentProperties, user.Properties);
        }

        var currentClaims = user.UserClaims
            .Where(x => !string.IsNullOrEmpty(x.ClaimType))
            .DistinctBy(x => new { x.ClaimType, x.ClaimValue })
            .ToList();

        var claimsChanged = false;
        if (context.ClaimsToRemove?.Count > 0)
        {
            var claimsToRemove = context.ClaimsToRemove.ToHashSet();
            foreach (var item in claimsToRemove)
            {
                var exists = currentClaims.FirstOrDefault(claim => claim.ClaimType == item.ClaimType && claim.ClaimValue == item.ClaimValue);
                if (exists is not null)
                {
                    currentClaims.Remove(exists);
                    claimsChanged = true;
                }
            }
        }

        if (context.ClaimsToUpdate?.Count > 0)
        {
            foreach (var item in context.ClaimsToUpdate)
            {
                var existing = currentClaims.FirstOrDefault(claim => claim.ClaimType == item.ClaimType && claim.ClaimValue == item.ClaimValue);
                if (existing is null)
                {
                    currentClaims.Add(item);
                    claimsChanged = true;
                }
            }
        }

        if (claimsChanged)
        {
            user.UserClaims = currentClaims;
            userNeedUpdate = true;
        }

        return userNeedUpdate;
    }

    private async Task<bool> AddConfirmEmailErrorAsync(IUser user)
    {
        var registrationFeatureIsAvailable = (await _shellFeaturesManager.GetAvailableFeaturesAsync())
            .Any(feature => feature.Id == UserConstants.Features.UserRegistration);

        if (!registrationFeatureIsAvailable)
        {
            return false;
        }

        var registrationSettings = await _siteService.GetSettingsAsync<RegistrationSettings>();
        if (registrationSettings.UsersMustValidateEmail)
        {
            // Require that the users have a confirmed email before they can log on.
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, S["You must confirm your email."]);
                return true;
            }
        }

        return false;
    }
}
