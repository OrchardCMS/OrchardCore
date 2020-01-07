using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<IAccountActivatedEventHandler> _handlers;
        private readonly INotifier _notifier;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<RegistrationController> S;
        private readonly IHtmlLocalizer<RegistrationController> H;

        public RegistrationController(
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            INotifier notifier,
            ILogger<RegistrationController> logger,
            IEnumerable<IAccountActivatedEventHandler> handlers,
            IHtmlLocalizer<RegistrationController> htmlLocalizer,
            IStringLocalizer<RegistrationController> stringLocalizer)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _siteService = siteService;
            _notifier = notifier;

            _handlers = handlers;

            _logger = logger;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            if (settings.UsersCanRegister != UserRegistrationType.AllowRegistration)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (settings.UsersCanRegister != UserRegistrationType.AllowRegistration)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;

            // If we get a user, redirect to returnUrl
            if (await this.RegisterUser(model, S["Confirm your account"], _logger) != null)
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(RegistrationController.Register), "Registration");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return View();
            }

            return NotFound();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id) as User;
            if (user != null)
            {
                await this.SendEmailConfirmationTokenAsync(user, S["Confirm your account"]);

                _notifier.Success(H["Verification email sent."]);
            }

            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateAccount([FromQuery]string email, [FromQuery]string activationToken, string returnUrl = null)
        {
            if (activationToken == null)
                return BadRequest(S["activationToken is null"]);

            if (email == null)
                return BadRequest(S["email is null"]);

            var user = await _userManager.FindByEmailAsync(email) as User;
            if (user == null)
                // Do not provide too much detail
                return BadRequest();
            
            if(await _userManager.HasPasswordAsync(user))
            {
                return await ActivateUserAccount(user, activationToken, null, returnUrl);
            }

            ViewData["returnUrl"] = returnUrl;

            var model = new AccountActivationViewModel
            {
                Email = email,
                ActivationToken = activationToken
            };

            return View("Activation", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateAccount(AccountActivationViewModel model, string returnUrl = null)
        {
            if (model == null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email) as User;
                if (user == null)
                    // Do not provide too much detail
                    return BadRequest();

                return await ActivateUserAccount(user, model.ActivationToken, model.Password, returnUrl);
            }

            return View("Activation", model);
        }

        private async Task<IActionResult> ActivateUserAccount(User user, string activationToken, string password = null, string returnUrl = null)
        {
            var isTokenValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.EmailConfirmationTokenProvider, "EmailConfirmation", activationToken);
            if (!isTokenValid)
                return BadRequest(S["Invalid token"]);

            user.IsEnabled = true;
            await _userManager.UpdateAsync(user);
            await _userManager.ConfirmEmailAsync(user, activationToken);

            if (password != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, password);
            }

            var context = new AccountActivatedContext(user);
            await _handlers.InvokeAsync((handler, context) => handler.AccountActivatedAsync(context), context, _logger);

            if (returnUrl == null)
            {
                return RedirectToLocal("~/");
            }
            else
            {
                return Redirect(returnUrl);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ActivateAccountConfirmation()
        {
            return View();
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

    }
}