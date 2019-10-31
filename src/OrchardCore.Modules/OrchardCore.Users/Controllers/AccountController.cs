using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace OrchardCore.Users.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;
        private readonly IScriptingManager _scriptingManager;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IClock _clock;
        private readonly IDistributedCache _distributedCache;


        public AccountController(
            IUserService userService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<AccountController> logger,
            ISiteService siteService,
            IStringLocalizer<AccountController> stringLocalizer,
            IEnumerable<ILoginFormEvent> accountEvents,
            IScriptingManager scriptingManager,
            IClock clock,
            IDistributedCache distributedCache,
            IDataProtectionProvider dataProtectionProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            _siteService = siteService;
            _accountEvents = accountEvents;
            _scriptingManager = scriptingManager;
            _clock = clock;
            _distributedCache = distributedCache;
            _dataProtectionProvider = dataProtectionProvider;
            T = stringLocalizer;
        }

        IStringLocalizer T { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
            {
                returnUrl = null;
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);


            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseExternalProviderIfOnlyOneDefined)
            {
                var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
                if (schemes.Count() == 1)
                {
                    var provider = schemes.First().Name;

                    var dataProtector = _dataProtectionProvider.CreateProtector(nameof(DefaultExternalLogin))
                                            .ToTimeLimitedDataProtector();

                    var token = Guid.NewGuid();
                    var expiration = new TimeSpan(0, 0, 5);
                    var protectedToken = dataProtector.Protect(token.ToString(), _clock.UtcNow.Add(expiration));
                    await _distributedCache.SetAsync(token.ToString(), token.ToByteArray(), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expiration });
                    return RedirectToAction(nameof(DefaultExternalLogin), new { protectedToken, returnUrl });
                }
            }

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
                    var dataProtector = _dataProtectionProvider.CreateProtector(nameof(DefaultExternalLogin))
                                            .ToTimeLimitedDataProtector();
                    try
                    {
                        Guid token;
                        if (Guid.TryParse(dataProtector.Unprotect(protectedToken), out token))
                        {
                            byte[] tokenBytes = await _distributedCache.GetAsync(token.ToString());
                            var cacheToken = new Guid(tokenBytes);
                            if (token.Equals(cacheToken))
                            {
                                return ExternalLogin(schemes.First().Name, returnUrl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured while validating DefaultExternalLogin token");
                    }
                }
            }
            return RedirectToAction(nameof(Login));
        }


        async Task<bool> AddConfirmEmailError(IUser user)
        {
            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            if (registrationSettings.UsersMustValidateEmail == true)
            {
                // Require that the users have a confirmed email before they can log on.
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, T["You must confirm your email."]);
                    return true;
                }
            }
            return false;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (ModelState.IsValid)
            {
                var disableLocalLogin = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>().DisableLocalLogin;
                if (disableLocalLogin)
                {
                    ModelState.AddModelError("", T["Local login is disabled."]);
                }
                else
                {
                    await _accountEvents.InvokeAsync(i => i.LoggingInAsync(model.UserName, (key, message) => ModelState.AddModelError(key, message)), _logger);
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    if (user != null)
                    {
                        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            if (!await AddConfirmEmailError(user))
                            {
                                result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

                                if (result.Succeeded)
                                {
                                    _logger.LogInformation(1, "User logged in.");
                                    await _accountEvents.InvokeAsync(a => a.LoggedInAsync(model.UserName), _logger);
                                    return RedirectToLocal(returnUrl);
                                }

                                ModelState.AddModelError(string.Empty, T["Invalid login attempt."]);
                                await _accountEvents.InvokeAsync(a => a.LoggingInFailedAsync(model.UserName), _logger);
                            }
                        }
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");

            return Redirect("~/");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetAuthenticatedUserAsync(User);
                if (await _userService.ChangePasswordAsync(user, model.CurrentPassword, model.Password, (key, message) => ModelState.AddModelError(key, message)))
                {
                    return RedirectToLocal(Url.Action("ChangePasswordConfirmation"));
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Could not get external login info.");
                return RedirectToAction(nameof(Login));
            }

            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user != null)
            {
                if (!await AddConfirmEmailError(user))
                {
                    await _accountEvents.InvokeAsync(i => i.LoggingInAsync(user.UserName, (key, message) => ModelState.AddModelError(key, message)), _logger);
                    var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                    if (signInResult.Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, T["Invalid login attempt."]);
                    }
                }
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                            ?? info.Principal.FindFirstValue(OpenIdConnectConstants.Claims.Email);

                if (!string.IsNullOrWhiteSpace(email))
                    user = await _userManager.FindByEmailAsync(email);

                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;

                if (user != null)
                {
                    // Link external login to an axisting user
                    ViewData["UserName"] = user.UserName;
                    ViewData["Email"] = email;

                    return View("LinkExternalLogin");
                }
                else
                {
                    // no user could be matched, check if a new user can register
                    if (registrationSettings.UsersCanRegister == RegistrationSettings.UsersCanRegisterEnum.NoRegistration)
                    {
                        string message = T["Site does not allow user registration."];
                        _logger.LogWarning(message);
                        ModelState.AddModelError("", message);
                    }
                    else
                    {
                        var externalLoginViewModel = new RegisterExternalLoginViewModel();

                        externalLoginViewModel.NoPassword = registrationSettings.NoPasswordForExternalUsers;
                        externalLoginViewModel.NoEmail = registrationSettings.NoEmailForExternalUsers;
                        externalLoginViewModel.NoUsername = registrationSettings.NoUsernameForExternalUsers;


                        externalLoginViewModel.UserName = await GenerateUsername(info);
                        externalLoginViewModel.Email = email;

                        // The user doesn't exist, if no information required, we can create the account locally
                        // instead of redirecting to the ExternalLogin
                        var noInformationRequired = externalLoginViewModel.NoPassword
                                                        && externalLoginViewModel.NoEmail
                                                        && externalLoginViewModel.NoUsername;

                        if (noInformationRequired)
                        {
                            user = await this.RegisterUser(new RegisterViewModel()
                            {
                                UserName = externalLoginViewModel.UserName,
                                Email = externalLoginViewModel.Email,
                                Password = null,
                                ConfirmPassword = null
                            }, T["Confirm your account"], _logger);

                            // If the registration was successfull we can link the external provider and redirect the user
                            if (user != null)
                            {
                                var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                                if (identityResult.Succeeded)
                                {
                                    _logger.LogInformation(3, "User account linked to {loginProvider} provider.", info.LoginProvider);

                                    // We have created/linked to the local user, so we must verify the login.
                                    // If it does not succeed, the user is not allowed to login
                                    var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                                    if (signInResult.Succeeded)
                                    {
                                        return RedirectToLocal(returnUrl);
                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, T["Invalid login attempt."]);
                                        return View(nameof(Login));
                                    }
                                }
                                AddIdentityErrors(identityResult);
                            }
                        }
                        return View("RegisterExternalLogin", externalLoginViewModel);
                    }
                }
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExternalLogin(RegisterExternalLoginViewModel model, string returnUrl = null)
        {
            IUser user = null;
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {

                _logger.LogWarning("Error loading external login info.");
                return NotFound();
            }

            if (settings.UsersCanRegister == RegistrationSettings.UsersCanRegisterEnum.NoRegistration)
            {
                _logger.LogWarning("Site does not allow user registration.", model.UserName, model.Email);
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            model.NoPassword = settings.NoPasswordForExternalUsers;
            model.NoEmail = settings.NoEmailForExternalUsers;
            model.NoUsername = settings.NoUsernameForExternalUsers;

            ModelState.Clear();

            if (model.NoEmail)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    ?? info.Principal.FindFirstValue(OpenIdConnectConstants.Claims.Email);
                model.Email = email;
            }

            if (model.NoUsername)
            {
                model.UserName = await GenerateUsername(info);
            }

            if (model.NoPassword)
            {
                model.Password = null;
                model.ConfirmPassword = null;
            }

            if (TryValidateModel(model) && ModelState.IsValid)
            {
                user = await this.RegisterUser(new RegisterViewModel() { UserName = model.UserName, Email = model.Email, Password = model.Password, ConfirmPassword = model.ConfirmPassword }, T["Confirm your account"], _logger);
                if (user is null)
                {
                    ModelState.AddModelError(string.Empty, "Registration Failed.");
                }
                else
                {
                    var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation(3, "User account linked to {provider} provider.", info.LoginProvider);
                        // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                        // the user is not allowed to login
                        if ((await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false)).Succeeded)
                        {
                            return RedirectToLocal(returnUrl);
                        }
                        else
                        {

                        }
                    }
                    AddIdentityErrors(identityResult);
                }
            }
            return View("RegisterExternalLogin", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkExternalLogin(LinkExternalLoginViewModel model, string returnUrl = null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var info = await _signInManager.GetExternalLoginInfoAsync();

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue(OpenIdConnectConstants.Claims.Email);

            var user = await _userManager.FindByEmailAsync(email);

            if (info == null)
            {
                _logger.LogWarning("Error loading external login info.");
                return NotFound();
            }


            if (user == null)
            {
                _logger.LogWarning("Suspicious login detected from external provider. {provider} with key [{providerKey}] for {identity}",
                    info.LoginProvider, info.ProviderKey, info.Principal?.Identity?.Name);
                return RedirectToAction(nameof(Login));
            }

            await _accountEvents.InvokeAsync(i => i.LoggingInAsync(user.UserName, (key, message) => ModelState.AddModelError(key, message)), _logger);

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!signInResult.Succeeded)
            {
                user = null;
                ModelState.AddModelError(string.Empty, T["Invalid login attempt."]);
            }
            else
            {
                var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                if (identityResult.Succeeded)
                {
                    _logger.LogInformation(3, "User account linked to {provider} provider.", info.LoginProvider);
                    // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                    // the user is not allowed to login
                    if ((await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false)).Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddIdentityErrors(identityResult);
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            //model.StatusMessage = StatusMessage;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
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

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //StatusMessage = "The external login was added.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("Unable to load user with ID '{UserId}'.", _userManager.GetUserId(User));
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                _logger.LogError("Unexpected error occurred adding external login info for user '{UserName}'.", user.UserName);
                return RedirectToAction(nameof(Login));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            //StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        async Task<string> GenerateUsername(ExternalLoginInfo info)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var claims = info == null ? "[]" : JsonConvert.SerializeObject(info.Principal.Claims.Select(c => new { c.Issuer, c.OriginalIssuer, c.Properties, c.Type, c.Value, c.ValueType }));

            var script = $"js: function generateUsernameFromExternalLoginInfo(provider,claims) {{\n{settings.GenerateUsernameScript}\n}}\nreturn generateUsernameFromExternalLoginInfo('{info?.LoginProvider}',{claims});";
            try
            {
                var evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                if (evaluationResult == null)
                    throw new Exception("GenerateUsernameScript did not return a username");
                return evaluationResult as string;
            }
            catch (Exception ex)
            {

                _logger.LogWarning("Suspicious login detected from external provider. {provider} with key [{providerKey}] for {identity}",
                    info.LoginProvider, info.ProviderKey, info.Principal?.Identity?.Name);

                _logger.LogError(ex, "Error evaluating GenerateUsernameScript( '{provider}',{claims} ) for {identity}",
                    info?.LoginProvider, claims, info.Principal?.Identity?.Name);
            }
            var now = (new TimeSpan(DateTime.UtcNow.Ticks) - new TimeSpan(DateTime.UnixEpoch.Ticks));
            return string.Concat("u" + Convert.ToInt32(now.TotalSeconds).ToString());
        }

    }
}