using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
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
    [Feature("OrchardCore.Users.ResetPassword")]
    [Admin]
    public class ResetPasswordController : BaseEmailController
    {
        private readonly IUserService _userService;
        private readonly UserManager<IUser> _userManager;
        private readonly ISiteService _siteService;

        public ResetPasswordController(
            IUserService userService,
            UserManager<IUser> userManager,
            ISiteService siteService,
            ISmtpService smtpService,
            IShapeFactory shapeFactory,
            IHtmlDisplay displayManager,
            IStringLocalizer<ResetPasswordController> stringLocalizer) : base(smtpService, shapeFactory, displayManager)
        {
            _userService = userService;
            _userManager = userManager;
            _siteService = siteService;

            T = stringLocalizer;
        }

        IStringLocalizer T { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userService.GetForgotPasswordUserAsync(model.UserIdentifier) as User;
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // returns to confirmation page anyway: we don't want to let scrapers know if a username or an email exist
                    return RedirectToLocal(Url.Action("ForgotPasswordConfirmation", "ResetPassword"));
                }

                user.ResetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ResetToken));
                var resetPasswordUrl = Url.Action("ResetPassword", "ResetPassword", new { code = user.ResetToken }, HttpContext.Request.Scheme);
                // send email with callback link
                await SendEmailAsync(user.Email, T["Reset your password"], new LostPasswordViewModel() { User = user, LostPasswordUrl = resetPasswordUrl }, "TemplateUserLostPassword");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }
            if (code == null)
            {
                //"A code must be supplied for password reset.";
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

            if (ModelState.IsValid)
            {
                if (await _userService.ResetPasswordAsync(model.Email, Encoding.UTF8.GetString(Convert.FromBase64String(model.ResetToken)), model.NewPassword, (key, message) => ModelState.AddModelError(key, message)))
                {
                    return RedirectToLocal(Url.Action("ResetPasswordConfirmation", "ResetPassword"));
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}