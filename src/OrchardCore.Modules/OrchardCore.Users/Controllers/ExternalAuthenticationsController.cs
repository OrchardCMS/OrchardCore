using System.Text.Json.Nodes;
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
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
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
    private readonly IEmailAddressValidator _emailAddressValidator;
    private readonly IUserService _userService;
    private readonly INotifier _notifier;
    private readonly IEnumerable<IExternalLoginEventHandler> _externalLoginHandlers;
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
        IEmailAddressValidator emailAddressValidator,
        IUserService userService,
        INotifier notifier,
        IEnumerable<IExternalLoginEventHandler> externalLoginHandlers,
        IOptions<ExternalLoginOptions> externalLoginOption,
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
        _emailAddressValidator = emailAddressValidator;
        _userService = userService;
        _notifier = notifier;
        _externalLoginHandlers = externalLoginHandlers;
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
            _logger.LogInformation("Found user using external provider and provider key.");

            await _accountEvents.InvokeAsync((e, user, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), iUser, ModelState, _logger);

            if (ModelState.IsValid)
            {
                foreach (var accountEvent in _accountEvents)
                {
                    var loginResult = await accountEvent.ValidatingLoginAsync(iUser);

                    if (loginResult != null)
                    {
                        return loginResult;
                    }
                }

                var signInResult = await ExternalSignInAsync(iUser, info);

                if (signInResult.Succeeded)
                {
                    await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), iUser, _logger);

                    return await LoggedInActionResultAsync(iUser, returnUrl, info);
                }
            }

            ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);

            return RedirectToLogin(returnUrl);
        }

        var email = info.GetEmail();

        if (_identityOptions.User.RequireUniqueEmail && !string.IsNullOrWhiteSpace(email))
        {
            iUser = await _userManager.FindByEmailAsync(email);
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["LoginProvider"] = info.LoginProvider;

        if (iUser != null)
        {
            _logger.LogInformation("Found external user using email. Attempt to link them to existing user.");

            foreach (var accountEvent in _accountEvents)
            {
                var loginResult = await accountEvent.ValidatingLoginAsync(iUser);

                if (loginResult != null)
                {
                    return loginResult;
                }
            }

            // Link external login to an existing user.
            ViewData["UserName"] = iUser.UserName;

            return View(nameof(LinkExternalLogin));
        }

        var settings = await _siteService.GetSettingsAsync<ExternalRegistrationSettings>();

        if (settings.DisableNewRegistrations)
        {
            await _notifier.ErrorAsync(H["New registrations are disabled for this site."]);

            return RedirectToLogin(returnUrl);
        }

        var externalLoginViewModel = new RegisterExternalLoginViewModel
        {
            NoPassword = settings.NoPassword,
            NoEmail = settings.NoEmail,
            NoUsername = settings.NoUsername,

            UserName = await GenerateUsernameAsync(info),
            Email = email,
        };

        // The user doesn't exist and no information required, we can create the account locally
        // instead of redirecting to the ExternalLogin.
        var noInformationRequired = settings.NoPassword &&
            settings.NoEmail &&
            settings.NoUsername;

        if (noInformationRequired)
        {
            _logger.LogInformation("Auto registering an external user using password-less method.");

            iUser = await _userService.RegisterAsync(new RegisterUserForm()
            {
                UserName = externalLoginViewModel.UserName,
                Email = externalLoginViewModel.Email,
                Password = null,
            }, ModelState.AddModelError);

            // If the registration was successful we can link the external provider and redirect the user.
            if (iUser == null || !ModelState.IsValid)
            {
                _logger.LogError("Unable to create internal account and link it to the external user.");

                await _notifier.ErrorAsync(H["Unable to create internal account and link it to the external user."]);

                return View(nameof(RegisterExternalLogin), externalLoginViewModel);
            }

            var identityResult = await _userManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));

            if (identityResult.Succeeded)
            {
                _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);

                // The login info must be linked before we consider a redirect, or the login info is lost.
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
                }

                // We have created/linked to the local user, so we must verify the login.
                // If it does not succeed, the user is not allowed to login
                var signInResult = await ExternalSignInAsync(iUser, info);

                if (signInResult.Succeeded)
                {
                    await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), iUser, _logger);

                    return await LoggedInActionResultAsync(iUser, returnUrl, info);
                }

                ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);

                return RedirectToLogin(returnUrl);
            }

            _logger.LogError("Unable to add external provider to a user: {LoginProvider} provider.", info.LoginProvider);

            AddErrorsToModelState(identityResult.Errors);
        }

        return View(nameof(RegisterExternalLogin), externalLoginViewModel);
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

        UpdateAndValidateEmail(model, info, settings.NoEmail);
        await UpdateAndValidateUserNameAsync(model, info, settings.NoUsername);
        await UpdateAndValidatePasswordAsync(model, settings.NoPassword);

        if (ModelState.IsValid)
        {
            var iUser = await _userService.RegisterAsync(
                new RegisterUserForm()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = model.Password,
                }, ModelState.AddModelError);

            if (iUser is not null && ModelState.IsValid)
            {
                var identityResult = await _userManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);

                    foreach (var accountEvent in _accountEvents)
                    {
                        var loginResult = await accountEvent.ValidatingLoginAsync(iUser);

                        if (loginResult != null)
                        {
                            return loginResult;
                        }
                    }

                    // we have created/linked to the local user, so we must verify the login.
                    // If it does not succeed, the user is not allowed to login
                    var signInResult = await ExternalSignInAsync(iUser, info);

                    if (signInResult.Succeeded)
                    {
                        return await LoggedInActionResultAsync(iUser, returnUrl, info);
                    }
                }

                AddErrorsToModelState(identityResult.Errors);
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

            return Redirect("~/");
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
                var identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);
                    // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                    // the user is not allowed to login.
                    if ((await ExternalSignInAsync(user, info)).Succeeded)
                    {
                        return await LoggedInActionResultAsync(user, returnUrl, info);
                    }
                }

                AddErrorsToModelState(identityResult.Errors);
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
        await ExternalSignInAsync(user, info);

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

            return Redirect("~/");
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

    private async Task UpdateAndValidatePasswordAsync(RegisterExternalLoginViewModel model, bool noPassword)
    {
        if (noPassword)
        {
            model.Password = null;
            model.ConfirmPassword = null;

            return;
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), S["Password is required!"]);
        }
        else if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), S["Confirm Password do not match"]);
        }
        else if (_userManager.PasswordValidators != null)
        {
            var user = new User();

            foreach (var passwordValidator in _userManager.PasswordValidators)
            {
                var validationResult = await passwordValidator.ValidateAsync(_userManager, user, model.Password);

                if (validationResult.Succeeded)
                {
                    continue;
                }

                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(nameof(model.Password), error.Description);
                }
            }
        }
    }

    private async Task UpdateAndValidateUserNameAsync(RegisterExternalLoginViewModel model, ExternalLoginInfo info, bool noUsername)
    {
        if (noUsername && string.IsNullOrWhiteSpace(model.UserName))
        {
            model.UserName = await GenerateUsernameAsync(info);
        }

        if (string.IsNullOrWhiteSpace(model.UserName))
        {
            ModelState.AddModelError(nameof(model.UserName), S["Username is required!"]);
        }
    }

    private void UpdateAndValidateEmail(RegisterExternalLoginViewModel model, ExternalLoginInfo info, bool noEmail)
    {
        if (noEmail && string.IsNullOrWhiteSpace(model.Email))
        {
            model.Email = info.GetEmail();
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), S["Email is required!"]);
        }
        else if (!_emailAddressValidator.Validate(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), S["Invalid Email."]);
        }
    }

    private async Task<Microsoft.AspNetCore.Identity.SignInResult> ExternalSignInAsync(IUser user, ExternalLoginInfo info)
    {
        _logger.LogInformation("Attempting to do an external sign in.");

        var externalClaims = info.Principal.GetSerializableClaims().ToArray();
        var userRoles = await _userManager.GetRolesAsync(user);
        var userInfo = user as User;

        var context = new UpdateUserContext(user, info.LoginProvider, externalClaims, userInfo.Properties.DeepClone() as JsonObject)
        {
            UserClaims = userInfo.UserClaims,
            UserRoles = userRoles,
        };

        foreach (var externalLoginHandlers in _externalLoginHandlers)
        {
            try
            {
                await externalLoginHandlers.UpdateUserAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The method {ExternalLoginHandler}.UpdateUserAsync(context) threw an exception", externalLoginHandlers.GetType());
            }
        }

        if (await _userManager.UpdateUserPropertiesAsync(userInfo, context))
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

                AddErrorsToModelState(identityResult.Errors);
            }
        }
        else
        {
            await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
        }

        return result;
    }

    private void AddErrorsToModelState(IEnumerable<IdentityError> errors)
    {
        foreach (var error in errors)
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
