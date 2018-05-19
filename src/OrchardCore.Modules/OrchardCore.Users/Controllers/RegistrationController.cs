using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
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
    public class RegistrationController : BaseEmailController
    {
        private readonly IUserService _userService;
        private readonly UserManager<IUser> _userManager;
        private readonly SignInManager<IUser> _signInManager;
        private readonly ISiteService _siteService;

        public RegistrationController(
            IUserService userService,
            UserManager<IUser> userManager,
            SignInManager<IUser> signInManager,
            ISiteService siteService,
            ISmtpService smtpService,
            IShapeFactory shapeFactory,
            IHtmlDisplay displayManager,
            ILogger<RegistrationController> logger,
            IStringLocalizer<RegistrationController> stringLocalizer) : base(smtpService, shapeFactory, displayManager)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _siteService = siteService;

            _logger = logger;
            T = stringLocalizer;
        }

        ILogger _logger;
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
                var user = (User)await _userService.CreateUserAsync(model.UserName, model.Email, new string[0], model.Password, !settings.UsersMustValidateEmail, (key, message) => ModelState.AddModelError(key, message));

                if (user != null)
                {
                    if (settings.UsersMustValidateEmail)
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        // Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Registration", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        await SendEmailAsync(user.Email, T["Confirm your account"], new ConfirmEmailViewModel() { User = user, ConfirmEmailUrl = callbackUrl }, "TemplateUserConfirmEmail");
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
                throw new Exception($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
    }
}