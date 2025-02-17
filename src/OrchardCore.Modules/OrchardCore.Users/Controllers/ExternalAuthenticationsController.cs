using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Feature(UserConstants.Features.ExternalAuthentication)]
public sealed class ExternalAuthenticationsController : AccountBaseController
{
    private readonly SignInManager<IUser> _signInManager;
    private readonly UserManager<IUser> _userManager;
    private readonly ILogger _logger;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IDistributedCache _distributedCache;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<ILoginFormEvent> _accountEvents;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly INotifier _notifier;
    private readonly IEnumerable<IExternalLoginEventHandler> _externalLoginHandlers;
    private readonly RegistrationOptions _registrationOptions;
    private readonly ExternalLoginOptions _externalLoginOption;
    private readonly IdentityOptions _identityOptions;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public ExternalAuthenticationsController(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ILogger<ExternalAuthenticationsController> logger,
        IDataProtectionProvider dataProtectionProvider,
        IDistributedCache distributedCache,
        ISiteService siteService,
        IHtmlLocalizer<ExternalAuthenticationsController> htmlLocalizer,
        IStringLocalizer<ExternalAuthenticationsController> stringLocalizer,
        IEnumerable<ILoginFormEvent> accountEvents,
        IShellFeaturesManager shellFeaturesManager,
        INotifier notifier,
        IEnumerable<IExternalLoginEventHandler> externalLoginHandlers,
        IOptions<ExternalLoginOptions> externalLoginOption,
        IOptions<RegistrationOptions> registrationOptions,
        IOptions<IdentityOptions> identityOptions)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _dataProtectionProvider = dataProtectionProvider;
        _distributedCache = distributedCache;
        _siteService = siteService;
        _accountEvents = accountEvents;
        _shellFeaturesManager = shellFeaturesManager;
        _notifier = notifier;
        _externalLoginHandlers = externalLoginHandlers;
        _registrationOptions = registrationOptions.Value;
        _externalLoginOption = externalLoginOption.Value;
        _identityOptions = identityOptions.Value;

        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogError("Error from external provider: {Error}", remoteError);
            ModelState.AddModelError(string.Empty, S["An error occurred in external provider."]);

            return RedirectToLogin(returnUrl);
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogError("Could not get external login info.");
            ModelState.AddModelError(string.Empty, S["An error occurred in external provider."]);

            return RedirectToLogin(returnUrl);
        }

        var iUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        CopyTempDataErrorsToModelState();

        if (iUser != null)
        {
            if (!await AddConfirmEmailErrorAsync(iUser) && !AddUserEnabledError(iUser, S))
            {
                await _accountEvents.InvokeAsync((e, user, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), iUser, ModelState, _logger);

                var signInResult = await ExternalLoginSignInAsync(iUser, info);
                if (signInResult.Succeeded)
                {
                    return await LoggedInActionResultAsync(iUser, returnUrl, info);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
                }
            }
        }
        else
        {
            var email = info.GetEmail();

            if (_identityOptions.User.RequireUniqueEmail && !string.IsNullOrWhiteSpace(email))
            {
                iUser = await _userManager.FindByEmailAsync(email);
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            if (iUser != null)
            {
                if (iUser is User userToLink && _registrationOptions.UsersMustValidateEmail && !userToLink.EmailConfirmed)
                {
                    return RedirectToAction(nameof(EmailConfirmationController.ConfirmEmailSent),
                        new
                        {
                            Area = UserConstants.Features.Users,
                            Controller = typeof(EmailConfirmationController).ControllerName(),
                            ReturnUrl = returnUrl,
                        });
                }

                // Link external login to an existing user.
                ViewData["UserName"] = iUser.UserName;

                return View(nameof(LinkExternalLogin));
            }

            var settings = await _siteService.GetSettingsAsync<ExternalRegistrationSettings>();

            if (settings.DisableNewRegistrations)
            {
                await _notifier.ErrorAsync(H["New registrations are disabled for this site."]);

                return RedirectToLogin();
            }

            var externalLoginViewModel = new RegisterExternalLoginViewModel
            {
                NoPassword = settings.NoPassword,
                NoEmail = settings.NoEmail,
                NoUsername = settings.NoUsername,

                UserName = await GenerateUsernameAsync(info),
                Email = info.GetEmail(),
            };

            // The user doesn't exist and no information required, we can create the account locally
            // instead of redirecting to the ExternalLogin.
            var noInformationRequired = settings.NoPassword &&
                settings.NoEmail &&
                settings.NoUsername;

            if (noInformationRequired)
            {
                iUser = await this.RegisterUser(new RegisterUserForm()
                {
                    UserName = externalLoginViewModel.UserName,
                    Email = externalLoginViewModel.Email,
                    Password = null,
                }, S["Confirm your account"], _logger);

                // If the registration was successful we can link the external provider and redirect the user.
                if (iUser != null)
                {
                    var identityResult = await _signInManager.UserManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);

                        // The login info must be linked before we consider a redirect, or the login info is lost.
                        if (iUser is User user)
                        {
                            if (_registrationOptions.UsersMustValidateEmail && !user.EmailConfirmed)
                            {
                                return RedirectToAction(nameof(EmailConfirmationController.ConfirmEmailSent),
                                    new
                                    {
                                        Area = UserConstants.Features.Users,
                                        Controller = typeof(EmailConfirmationController).ControllerName(),
                                        ReturnUrl = returnUrl,
                                    });
                            }

                            if (_registrationOptions.UsersAreModerated && !user.IsEnabled)
                            {
                                return RedirectToAction(nameof(RegistrationController.RegistrationPending),
                                    new
                                    {
                                        Area = UserConstants.Features.Users,
                                        Controller = typeof(RegistrationController).ControllerName(),
                                        ReturnUrl = returnUrl,
                                    });
                            }
                        }

                        // We have created/linked to the local user, so we must verify the login.
                        // If it does not succeed, the user is not allowed to login
                        var signInResult = await ExternalLoginSignInAsync(iUser, info);
                        if (signInResult.Succeeded)
                        {
                            return await LoggedInActionResultAsync(iUser, returnUrl, info);
                        }

                        ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);

                        return RedirectToLogin(returnUrl);
                    }

                    AddIdentityErrors(identityResult);
                }
            }

            return View(nameof(RegisterExternalLogin), externalLoginViewModel);
        }

        return RedirectToLogin(returnUrl);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterExternalLogin(RegisterExternalLoginViewModel model, string returnUrl = null)
    {
        var settings = await _siteService.GetSettingsAsync<ExternalRegistrationSettings>();

        if (settings.DisableNewRegistrations)
        {
            await _notifier.ErrorAsync(H["New registrations are disabled for this site."]);

            return RedirectToLogin(returnUrl);
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            _logger.LogWarning("Error loading external login info.");

            return NotFound();
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["LoginProvider"] = info.LoginProvider;

        model.NoPassword = settings.NoPassword;
        model.NoEmail = settings.NoEmail;
        model.NoUsername = settings.NoUsername;

        ModelState.Clear();

        if (model.NoEmail && string.IsNullOrWhiteSpace(model.Email))
        {
            model.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
        }

        if (model.NoUsername && string.IsNullOrWhiteSpace(model.UserName))
        {
            model.UserName = await GenerateUsernameAsync(info);
        }

        if (model.NoPassword)
        {
            model.Password = null;
            model.ConfirmPassword = null;
        }

        if (TryValidateModel(model) && ModelState.IsValid)
        {
            var iUser = await this.RegisterUser(
                new RegisterUserForm()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = model.Password,
                }, S["Confirm your account"], _logger);

            if (iUser is null)
            {
                ModelState.AddModelError(string.Empty, "Registration Failed.");
            }
            else
            {
                var identityResult = await _signInManager.UserManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                if (identityResult.Succeeded)
                {
                    _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);

                    // The login info must be linked before we consider a redirect, or the login info is lost.
                    if (iUser is User user)
                    {
                        if (_registrationOptions.UsersMustValidateEmail && !user.EmailConfirmed)
                        {
                            return RedirectToAction(nameof(EmailConfirmationController.ConfirmEmailSent),
                                new
                                {
                                    Area = UserConstants.Features.Users,
                                    Controller = typeof(EmailConfirmationController).ControllerName(),
                                    ReturnUrl = returnUrl,
                                });
                        }

                        if (_registrationOptions.UsersAreModerated && !user.IsEnabled)
                        {
                            return RedirectToAction(nameof(RegistrationController.RegistrationPending),
                                new
                                {
                                    Area = UserConstants.Features.Users,
                                    Controller = typeof(RegistrationController).ControllerName(),
                                    ReturnUrl = returnUrl,
                                });
                        }
                    }

                    // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                    // the user is not allowed to login
                    var signInResult = await ExternalLoginSignInAsync(iUser, info);
                    if (signInResult.Succeeded)
                    {
                        return await LoggedInActionResultAsync(iUser, returnUrl, info);
                    }
                }

                AddIdentityErrors(identityResult);
            }
        }

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkExternalLogin(LinkExternalLoginViewModel model, string returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            _logger.LogWarning("Error loading external login info.");

            return NotFound();
        }

        var user = await _userManager.FindByEmailAsync(info.GetEmail());

        if (user == null)
        {
            _logger.LogWarning("Suspicious login detected from external provider. {LoginProvider} with key [{ProviderKey}] for {Identity}",
                info.LoginProvider, info.ProviderKey, info.Principal?.Identity?.Name);

            return RedirectToLogin();
        }

        if (ModelState.IsValid)
        {
            await _accountEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!signInResult.Succeeded)
            {
                user = null;
                ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
            }
            else
            {
                var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                if (identityResult.Succeeded)
                {
                    _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);
                    // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                    // the user is not allowed to login.
                    if ((await ExternalLoginSignInAsync(user, info)).Succeeded)
                    {
                        return await LoggedInActionResultAsync(user, returnUrl, info);
                    }
                }
                AddIdentityErrors(identityResult);
            }
        }

        CopyModelStateErrorsToTempData(null);

        return RedirectToLogin();
    }

    public async Task<IActionResult> ExternalLogins()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Forbid();
        }

        var model = new ExternalLoginsViewModel
        {
            CurrentLogins = await _userManager.GetLoginsAsync(user),
        };
        model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
        model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToArray();

        CopyTempDataErrorsToModelState();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkLogin(string provider)
    {
        // Clear the existing external cookie to ensure a clean login process.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Request a redirect to the external login provider to link a login for the current user.
        var redirectUrl = Url.Action(nameof(LinkLoginCallback));
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));

        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> LinkLoginCallback()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogError("Unable to load user with ID '{UserId}'.", _userManager.GetUserId(User));

            return RedirectToLogin();
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogError("Unexpected error occurred loading external login info for user '{UserName}'.", user.UserName);

            return RedirectToLogin();
        }

        var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
        if (!result.Succeeded)
        {
            _logger.LogError("Unexpected error occurred adding external login info for user '{UserName}'.", user.UserName);

            return RedirectToLogin();
        }

        // Clear the existing external cookie to ensure a clean login process.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        // Perform External Login SignIn.
        await ExternalLoginSignInAsync(user, info);

        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user is not User u)
        {
            _logger.LogError("Unable to load user with ID '{UserId}'.", _userManager.GetUserId(User));

            return RedirectToLogin();
        }

        var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
        if (!result.Succeeded)
        {
            _logger.LogError("Unexpected error occurred removing external login info for user '{UserName}'.", user.UserName);

            return RedirectToLogin();
        }

        // Remove External Authentication Tokens.
        foreach (var item in u.UserTokens.Where(c => c.LoginProvider == model.LoginProvider).ToList())
        {
            if (!(await _userManager.RemoveAuthenticationTokenAsync(user, model.LoginProvider, item.Name)).Succeeded)
            {
                _logger.LogError("Could not remove '{TokenName}' token while unlinking '{LoginProvider}' provider from user '{UserName}'.", item.Name, model.LoginProvider, user.UserName);
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction(nameof(ExternalLogins));
    }

    private async Task<Microsoft.AspNetCore.Identity.SignInResult> ExternalLoginSignInAsync(IUser user, ExternalLoginInfo info)
    {
        var externalClaims = info.Principal.GetSerializableClaims();
        var userRoles = await _userManager.GetRolesAsync(user);
        var userInfo = user as User;

        var context = new UpdateUserContext(user, info.LoginProvider, externalClaims, userInfo.Properties)
        {
            UserClaims = userInfo.UserClaims,
            UserRoles = userRoles,
        };
        foreach (var item in _externalLoginHandlers)
        {
            try
            {
                await item.UpdateUserAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ExternalLoginHandler}.UpdateUserAsync threw an exception", item.GetType());
            }
        }

        if (await UserManagerHelper.UpdateUserPropertiesAsync(_userManager, userInfo, context))
        {
            await _userManager.UpdateAsync(user);
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

            var identityResult = await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
            if (!identityResult.Succeeded)
            {
                _logger.LogError("Error updating the external authentication tokens.");
            }
        }
        else
        {
            await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
        }

        return result;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private RedirectToActionResult RedirectToLogin(string returnUrl)
    {
        CopyModelStateErrorsToTempData();

        return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName(), new { returnUrl });
    }

    private RedirectToActionResult RedirectToLogin()
        => RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());

    private void CopyModelStateErrorsToTempData(string key = "")
    {
        var iix = 0;

        foreach (var state in ModelState)
        {
            if (key != null && state.Key != key)
            {
                continue;
            }

            foreach (var item in state.Value.Errors)
            {
                TempData[$"error_{iix++}"] = item.ErrorMessage;
            }
        }
    }

    private async Task<bool> AddConfirmEmailErrorAsync(IUser user)
    {
        if (_registrationOptions.UsersMustValidateEmail)
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

    private async Task<string> GenerateUsernameAsync(ExternalLoginInfo info)
    {
        var ret = string.Concat("u", IdGenerator.GenerateId());
        var externalClaims = info?.Principal.GetSerializableClaims();
        var userNames = new Dictionary<Type, string>();

        foreach (var item in _externalLoginHandlers)
        {
            try
            {
                var userName = await item.GenerateUserName(info.LoginProvider, externalClaims.ToArray());
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    // Set the return value to the username generated by the first IExternalLoginHandler.
                    if (userNames.Count == 0)
                    {
                        ret = userName;
                    }
                    userNames.Add(item.GetType(), userName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ExternalLoginHandler} - IExternalLoginHandler.GenerateUserName threw an exception", item.GetType());
            }
        }

        if (userNames.Count > 1)
        {
            _logger.LogWarning("More than one IExternalLoginHandler generated username. Used first one registered, {ExternalLoginHandler}", userNames.FirstOrDefault().Key);
        }

        return ret;
    }
}
