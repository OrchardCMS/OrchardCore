using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Feature("OrchardCore.Users.ResetPassword")]
    public class ResetPasswordController : Controller
    {
        private static readonly string _controllerName = typeof(ResetPasswordController).ControllerName();

        private readonly IUserService _userService;
        private readonly UserManager<IUser> _userManager;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<IPasswordRecoveryFormEvents> _passwordRecoveryFormEvents;
        private readonly ILogger _logger;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IDisplayManager<ForgotPasswordForm> _displayManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;

        protected readonly IStringLocalizer S;

        public ResetPasswordController(
            IUserService userService,
            UserManager<IUser> userManager,
            ISiteService siteService,
            ILogger<ResetPasswordController> logger,
            IUpdateModelAccessor updateModelAccessor,
            IDisplayManager<ForgotPasswordForm> displayManager,
            IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IPasswordRecoveryFormEvents> passwordRecoveryFormEvents,
            IStringLocalizer<ResetPasswordController> stringLocalizer)
        {
            _userService = userService;
            _userManager = userManager;
            _siteService = siteService;
            _logger = logger;
            _updateModelAccessor = updateModelAccessor;
            _displayManager = displayManager;
            _shellFeaturesManager = shellFeaturesManager;
            _passwordRecoveryFormEvents = passwordRecoveryFormEvents;
            S = stringLocalizer;
        }

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            var formShape = await _displayManager.BuildEditorAsync(_updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

            return View(formShape);
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName(nameof(ForgotPassword))]
        public async Task<IActionResult> ForgotPasswordPOST()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            var model = new ForgotPasswordForm();

            var formShape = await _displayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

            await _passwordRecoveryFormEvents.InvokeAsync((e, modelState) => e.RecoveringPasswordAsync((key, message) => modelState.AddModelError(key, message)), ModelState, _logger);

            if (ModelState.IsValid)
            {
                var user = await _userService.GetForgotPasswordUserAsync(model.Identifier) as User;
                if (user == null || await MustValidateEmailAsync(user))
                {
                    // returns to confirmation page anyway: we don't want to let scrapers know if a username or an email exist
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                user.ResetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ResetToken));
                var resetPasswordUrl = Url.Action(nameof(ResetPassword), _controllerName, new { code = user.ResetToken }, HttpContext.Request.Scheme);

                // send email with callback link
                await this.SendEmailAsync(user.Email, S["Reset your password"], new LostPasswordViewModel()
                {
                    User = user,
                    LostPasswordUrl = resetPasswordUrl
                });

                var context = new PasswordRecoveryContext(user);

                await _passwordRecoveryFormEvents.InvokeAsync((handler, context) => handler.PasswordRecoveredAsync(context), context, _logger);

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(formShape);
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }
            if (code == null)
            {
                // "A code must be supplied for password reset.";
            }
            return View(new ResetPasswordViewModel { ResetToken = code });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            await _passwordRecoveryFormEvents.InvokeAsync((e, modelState) => e.ResettingPasswordAsync((key, message) => modelState.AddModelError(key, message)), ModelState, _logger);

            if (TryValidateModel(model) && ModelState.IsValid)
            {
                if (await _userService.ResetPasswordAsync(model.Email, Encoding.UTF8.GetString(Convert.FromBase64String(model.ResetToken)), model.NewPassword, (key, message) => ModelState.AddModelError(key == "Password" ? nameof(ResetPasswordViewModel.NewPassword) : key, message)))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private async Task<bool> MustValidateEmailAsync(User user)
        {
            var registrationFeatureIsAvailable = (await _shellFeaturesManager.GetAvailableFeaturesAsync())
                           .Any(feature => feature.Id == "OrchardCore.Users.Registration");

            if (!registrationFeatureIsAvailable)
            {
                return false;
            }

            return (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersMustValidateEmail
                && !await _userManager.IsEmailConfirmedAsync(user);
        }
    }
}
