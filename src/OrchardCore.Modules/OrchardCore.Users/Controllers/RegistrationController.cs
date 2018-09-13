using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Feature("OrchardCore.Users.Registration")]
    [Admin]
    public class RegistrationController : BaseEmailController
    {
        private readonly IUserService _userService;
        private readonly UserManager<IUser> _userManager;
        private readonly SignInManager<IUser> _signInManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;

        private readonly INotifier _notifier;

        public RegistrationController(
            IUserService userService,
            UserManager<IUser> userManager,
            SignInManager<IUser> signInManager,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            INotifier notifier,
            ISmtpService smtpService,
            IShapeFactory shapeFactory,
            IHtmlDisplay displayManager,
            ILogger<RegistrationController> logger,
            IHtmlLocalizer<RegistrationController> htmlLocalizer,
            IStringLocalizer<RegistrationController> stringLocalizer) : base(smtpService, shapeFactory, displayManager)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _authorizationService = authorizationService;
            _siteService = siteService;
            _notifier = notifier;

            _logger = logger;
            TH = htmlLocalizer;
            T = stringLocalizer;
        }

        ILogger _logger;
        IHtmlLocalizer TH { get; set; }
        IStringLocalizer T { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister)
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

            if (!settings.UsersCanRegister)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userService.CreateUserAsync(new User { UserName = model.UserName, Email = model.Email, EmailConfirmed = !settings.UsersMustValidateEmail, RoleNames = new string[0] }, model.Password, (key, message) => ModelState.AddModelError(key, message)) as User;
                
                if (user != null)
                {
                    if (settings.UsersMustValidateEmail)
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        // Send an email with this link
                        await SendEmailConfirmationTokenAsync(user);
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                    }
                    _logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }
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
                return  NotFound();
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
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
                await SendEmailConfirmationTokenAsync(user);

                _notifier.Success(TH["Verification email sent."]);
            }

            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }

        private async Task<string> SendEmailConfirmationTokenAsync(User user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Registration", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await SendEmailAsync(user.Email, T["Confirm your account"], new ConfirmEmailViewModel() { User = user, ConfirmEmailUrl = callbackUrl }, "TemplateUserConfirmEmail");

            return callbackUrl;
        }
    }
}