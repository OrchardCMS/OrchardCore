using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace OrchardCore.Users.Controllers
{
    [Authorize]
    public class AccountController : AccountBaseController
    {
        public const string DefaultExternalLoginProtector = "DefaultExternalLogin";

        private readonly IUserService _userService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly INotifier _notifier;
        private readonly IClock _clock;
        private readonly IDistributedCache _distributedCache;
        private readonly IEnumerable<IExternalLoginEventHandler> _externalLoginHandlers;
        protected readonly IHtmlLocalizer H;
        protected readonly IStringLocalizer S;

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
            IEnumerable<IExternalLoginEventHandler> externalLoginHandlers)
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
            _externalLoginHandlers = externalLoginHandlers;

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

            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
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

                    return RedirectToAction(nameof(DefaultExternalLogin), new { protectedToken, returnUrl });
                }
            }

            CopyTempDataErrorsToModelState();
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DefaultExternalLogin(string protectedToken, string returnUrl = null)
        {
            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseExternalProviderIfOnlyOneDefined)
            {
                var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
                if (schemes.Count() == 1)
                {
                    var dataProtector = _dataProtectionProvider.CreateProtector(DefaultExternalLoginProtector)
                        .ToTimeLimitedDataProtector();

                    try
                    {
                        if (Guid.TryParse(dataProtector.Unprotect(protectedToken), out var token))
                        {
                            var tokenBytes = await _distributedCache.GetAsync(token.ToString());
                            var cacheToken = new Guid(tokenBytes);
                            if (token.Equals(cacheToken))
                            {
                                return ExternalLogin(schemes.First().Name, returnUrl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while validating {defaultExternalLogin} token", DefaultExternalLoginProtector);
                    }
                }
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (TryValidateModel(model) && ModelState.IsValid)
            {
                var disableLocalLogin = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>().DisableLocalLogin;
                if (disableLocalLogin)
                {
                    ModelState.AddModelError(String.Empty, S["Local login is disabled."]);
                }
                else
                {
                    await _accountEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(model.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);
                    if (ModelState.IsValid)
                    {
                        var user = await _userService.GetUserAsync(model.UserName);
                        if (user != null)
                        {
                            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
                            if (result.Succeeded)
                            {
                                if (!await AddConfirmEmailErrorAsync(user) && !AddUserEnabledError(user))
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
                                ModelState.AddModelError(String.Empty, S["The account is locked out"]);
                                await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                                return View();
                            }

                            // Login failed with a known user.
                            await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
                        }

                        ModelState.AddModelError(String.Empty, S["Invalid login attempt."]);
                    }

                    // Login failed unknown user.
                    await _accountEvents.InvokeAsync((e, model) => e.LoggingInFailedAsync(model.UserName), model, _logger);
                }
            }

            // If we got this far, something failed, redisplay form.
            return View(model);
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
                if (await _userService.ChangePasswordAsync(user, model.CurrentPassword, model.Password, (key, message) => ModelState.AddModelError(key, message)))
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

        private async Task<SignInResult> ExternalLoginSignInAsync(IUser user, ExternalLoginInfo info)
        {
            var claims = info.Principal.GetSerializableClaims();
            var userRoles = await _userManager.GetRolesAsync(user);
            var context = new UpdateRolesContext(user, info.LoginProvider, claims, userRoles);

            foreach (var item in _externalLoginHandlers)
            {
                try
                {
                    await item.UpdateRoles(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{externalLoginHandler} - IExternalLoginHandler.UpdateRoles threw an exception", item.GetType());
                }
            }

            await _userManager.AddToRolesAsync(user, context.RolesToAdd.Distinct());
            await _userManager.RemoveFromRolesAsync(user, context.RolesToRemove.Distinct());

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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                ModelState.AddModelError(String.Empty, S["An error occurred in external provider."]);

                return RedirectToLogin(returnUrl);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Could not get external login info.");
                ModelState.AddModelError(String.Empty, S["An error occurred in external provider."]);

                return RedirectToLogin(returnUrl);
            }

            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var iUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            CopyTempDataErrorsToModelState();

            if (iUser != null)
            {
                if (!await AddConfirmEmailErrorAsync(iUser) && !AddUserEnabledError(iUser))
                {
                    await _accountEvents.InvokeAsync((e, user, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), iUser, ModelState, _logger);

                    var signInResult = await ExternalLoginSignInAsync(iUser, info);
                    if (signInResult.Succeeded)
                    {
                        return await LoggedInActionResultAsync(iUser, returnUrl, info);
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, S["Invalid login attempt."]);
                    }
                }
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

                if (!String.IsNullOrWhiteSpace(email))
                {
                    iUser = await _userManager.FindByEmailAsync(email);
                }

                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;

                if (iUser != null)
                {
                    if (iUser is User userToLink && registrationSettings.UsersMustValidateEmail && !userToLink.EmailConfirmed)
                    {
                        return RedirectToAction(nameof(RegistrationController.ConfirmEmailSent),
                            new
                            {
                                Area = "OrchardCore.Users",
                                Controller = typeof(RegistrationController).ControllerName(),
                                ReturnUrl = returnUrl,
                            });
                    }

                    // Link external login to an existing user
                    ViewData["UserName"] = iUser.UserName;
                    ViewData["Email"] = email;

                    return View(nameof(LinkExternalLogin));
                }

                // No user could be matched, check if a new user can register.
                if (registrationSettings.UsersCanRegister == UserRegistrationType.NoRegistration)
                {
                    var message = S["Site does not allow user registration."];
                    _logger.LogWarning("Site does not allow user registration.");
                    ModelState.AddModelError(String.Empty, message);
                }
                else
                {
                    var externalLoginViewModel = new RegisterExternalLoginViewModel
                    {
                        NoPassword = registrationSettings.NoPasswordForExternalUsers,
                        NoEmail = registrationSettings.NoEmailForExternalUsers,
                        NoUsername = registrationSettings.NoUsernameForExternalUsers,

                        // If registrationSettings.NoUsernameForExternalUsers is true, this username will not be used
                        UserName = await GenerateUsernameAsync(info),
                        Email = email
                    };

                    // The user doesn't exist and no information required, we can create the account locally
                    // instead of redirecting to the ExternalLogin.
                    var noInformationRequired = externalLoginViewModel.NoPassword
                        && externalLoginViewModel.NoEmail
                        && externalLoginViewModel.NoUsername;

                    if (noInformationRequired)
                    {
                        iUser = await this.RegisterUser(new RegisterViewModel()
                        {
                            UserName = externalLoginViewModel.UserName,
                            Email = externalLoginViewModel.Email,
                            Password = null,
                            ConfirmPassword = null
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
                                    if (registrationSettings.UsersMustValidateEmail && !user.EmailConfirmed)
                                    {
                                        return RedirectToAction(nameof(RegistrationController.ConfirmEmailSent),
                                            new
                                            {
                                                Area = "OrchardCore.Users",
                                                Controller = typeof(RegistrationController).ControllerName(),
                                                ReturnUrl = returnUrl,
                                            });
                                    }

                                    if (registrationSettings.UsersAreModerated && !user.IsEnabled)
                                    {
                                        return RedirectToAction(nameof(RegistrationController.RegistrationPending),
                                            new
                                            {
                                                Area = "OrchardCore.Users",
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

                                ModelState.AddModelError(String.Empty, S["Invalid login attempt."]);

                                return RedirectToLogin(returnUrl);
                            }

                            AddIdentityErrors(identityResult);
                        }
                    }

                    return View(nameof(RegisterExternalLogin), externalLoginViewModel);
                }
            }

            return RedirectToLogin(returnUrl);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExternalLogin(RegisterExternalLoginViewModel model, string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                _logger.LogWarning("Error loading external login info.");

                return NotFound();
            }

            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (settings.UsersCanRegister == UserRegistrationType.NoRegistration)
            {
                _logger.LogWarning("Site does not allow user registration.");

                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            model.NoPassword = settings.NoPasswordForExternalUsers;
            model.NoEmail = settings.NoEmailForExternalUsers;
            model.NoUsername = settings.NoUsernameForExternalUsers;

            ModelState.Clear();

            if (model.NoEmail && String.IsNullOrWhiteSpace(model.Email))
            {
                model.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
            }

            if (model.NoUsername && String.IsNullOrWhiteSpace(model.UserName))
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
                    new RegisterViewModel()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        Password = model.Password,
                        ConfirmPassword = model.ConfirmPassword
                    }, S["Confirm your account"], _logger);

                if (iUser is null)
                {
                    ModelState.AddModelError(String.Empty, "Registration Failed.");
                }
                else
                {
                    var identityResult = await _signInManager.UserManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation(3, "User account linked to {provider} provider.", info.LoginProvider);

                        // The login info must be linked before we consider a redirect, or the login info is lost.
                        if (iUser is User user)
                        {
                            if (settings.UsersMustValidateEmail && !user.EmailConfirmed)
                            {
                                return RedirectToAction(nameof(RegistrationController.ConfirmEmailSent),
                                    new
                                    {
                                        Area = "OrchardCore.Users",
                                        Controller = typeof(RegistrationController).ControllerName(),
                                        ReturnUrl = returnUrl,
                                    });
                            }

                            if (settings.UsersAreModerated && !user.IsEnabled)
                            {
                                return RedirectToAction(nameof(RegistrationController.RegistrationPending),
                                    new
                                    {
                                        Area = "OrchardCore.Users",
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

            return View(nameof(RegisterExternalLogin), model);
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
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Suspicious login detected from external provider. {provider} with key [{providerKey}] for {identity}",
                    info.LoginProvider, info.ProviderKey, info.Principal?.Identity?.Name);

                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                await _accountEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!signInResult.Succeeded)
                {
                    user = null;
                    ModelState.AddModelError(String.Empty, S["Invalid login attempt."]);
                }
                else
                {
                    var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation(3, "User account linked to {provider} provider.", info.LoginProvider);
                        // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                        // the user is not allowed to login
                        if ((await ExternalLoginSignInAsync(user, info)).Succeeded)
                        {
                            return await LoggedInActionResultAsync(user, returnUrl, info);
                        }
                    }
                    AddIdentityErrors(identityResult);
                }
            }

            CopyModelStateErrorsToTempData(null);

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            //model.StatusMessage = StatusMessage;

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

        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("Unable to load user with ID '{UserId}'.", _userManager.GetUserId(User));

                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Unexpected error occurred loading external login info for user '{UserName}'.", user.UserName);

                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            if (!result.Succeeded)
            {
                _logger.LogError("Unexpected error occurred adding external login info for user '{UserName}'.", user.UserName);

                return RedirectToAction(nameof(Login));
            }

            // Clear the existing external cookie to ensure a clean login process.
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Perform External Login SignIn.
            await ExternalLoginSignInAsync(user, info);
            //StatusMessage = "The external login was added.";

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

                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                _logger.LogError("Unexpected error occurred removing external login info for user '{UserName}'.", user.UserName);

                return RedirectToAction(nameof(Login));
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
            //StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        private async Task<string> GenerateUsernameAsync(ExternalLoginInfo info)
        {
            var ret = String.Concat("u", IdGenerator.GenerateId());
            var externalClaims = info?.Principal.GetSerializableClaims();
            var userNames = new Dictionary<Type, string>();

            foreach (var item in _externalLoginHandlers)
            {
                try
                {
                    var userName = await item.GenerateUserName(info.LoginProvider, externalClaims.ToArray());
                    if (!String.IsNullOrWhiteSpace(userName))
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
                    _logger.LogError(ex, "{externalLoginHandler} - IExternalLoginHandler.GenerateUserName threw an exception", item.GetType());
                }
            }

            if (userNames.Count > 1)
            {
                _logger.LogWarning("More than one IExternalLoginHandler generated username. Used first one registered, {externalLoginHandler}", userNames.FirstOrDefault().Key);
            }

            return ret;
        }

        private RedirectToActionResult RedirectToLogin(string returnUrl)
        {
            CopyModelStateErrorsToTempData();

            return RedirectToAction(nameof(Login), new { returnUrl });
        }

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

        private void CopyTempDataErrorsToModelState()
        {
            foreach (var errorMessage in TempData.Where(x => x.Key.StartsWith("error")).Select(x => x.Value.ToString()))
            {
                ModelState.AddModelError(String.Empty, errorMessage);
            }
        }

        private bool AddUserEnabledError(IUser user)
        {
            if (user is not User localUser || !localUser.IsEnabled)
            {
                ModelState.AddModelError(String.Empty, S["The specified user is not allowed to sign in."]);

                return true;
            }

            return false;
        }

        private async Task<bool> AddConfirmEmailErrorAsync(IUser user)
        {
            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            if (registrationSettings.UsersMustValidateEmail)
            {
                // Require that the users have a confirmed email before they can log on.
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(String.Empty, S["You must confirm your email."]);
                    return true;
                }
            }

            return false;
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }
    }
}
