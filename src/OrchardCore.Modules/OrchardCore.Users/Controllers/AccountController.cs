using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Authorize]
    [Admin]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISiteService _siteService;

        public AccountController(
            IUserService userService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<AccountController> logger,
            ISiteService siteService,
            IStringLocalizer<AccountController> stringLocalizer)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            _siteService = siteService;

            T = stringLocalizer;
        }

        IStringLocalizer T { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if ((await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersMustValidateEmail)
            {
                // Require that the users have a confirmed email before they can log on.
                var user = await _userManager.FindByNameAsync(model.UserName) as User;
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, T["You must have a confirmed email to log on."]);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning(2, "User account locked out.");
                //    return View("Lockout");
                //}
                else
                {
                    ModelState.AddModelError(string.Empty, T["Invalid login attempt."]);
                    return View(model);
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

        private void AddErrors(IdentityResult result)
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
        public ActionResult ExternalLogin(string provider, string returnUrl = null)
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
                _logger.LogError($"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError($"Could not get external login info");
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogError($"User is locked out");
                return RedirectToAction(nameof(Login));
                //return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ExternalLoginViewModel model = new ExternalLoginViewModel();
                IUser existingUser = null;

                model.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue(OpenIdConnectConstants.Claims.Email);
                if (model.Email != null)
                    existingUser = await _userManager.FindByEmailAsync(model.Email);

                if (model.IsExistingUser = (existingUser != null))
                    model.UserName = existingUser.UserName;
                else
                    model.UserName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? info.Principal.FindFirstValue(OpenIdConnectConstants.Claims.GivenName) ?? info.Principal.FindFirstValue(OpenIdConnectConstants.Claims.Name);

                // If the user does not have an account, check if he can create an account.
                if ((!model.IsExistingUser && !(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister))
                {
                    _logger.LogInformation("Site does not allow user registration.");
                    return NotFound();
                }

                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;

                return View("ExternalLogin", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null, string loginProvider = null)
        {
            if ((!model.IsExistingUser && !(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister))
            {
                _logger.LogInformation("Site does not allow user registration.");
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = loginProvider;

            if (ModelState.IsValid)
            {
                IUser user = null;
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }

                if (!model.IsExistingUser)
                {
                    user = new User() { UserName = model.UserName, Email = model.Email };
                    user = await _userService.CreateUserAsync(user, model.Password, (key, message) => ModelState.AddModelError(key, message));
                    _logger.LogInformation(3, "User created an account with password.");
                }
                else
                {
                    user = await _userManager.FindByNameAsync(model.UserName);
                }

                if (user != null)
                {
                    var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (!signInResult.Succeeded)
                    {
                        user = null;
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }

                    var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                    if (identityResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(3, "User account linked to {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(identityResult);
                }
            }
            return View(nameof(ExternalLogin), model);
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
                _logger.LogError($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError($"Unexpected error occurred loading external login info for user '{user.UserName}'.");
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            if (!result.Succeeded)
            {
                _logger.LogError($"Unexpected error occurred adding external login info for user '{user.UserName}'.");
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
                _logger.LogError($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                _logger.LogError($"Unexpected error occurred adding external login info for user '{user.UserName}'.");
                return RedirectToAction(nameof(Login));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            //StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(ExternalLogins));
        }

    }
}