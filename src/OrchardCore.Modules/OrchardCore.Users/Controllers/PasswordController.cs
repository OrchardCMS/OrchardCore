using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
    [Feature("OrchardCore.Users.Password")]
    public class PasswordController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly ISmtpService _smtpService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IHtmlDisplay _displayManager;

        public PasswordController(
            IUserService userService,
            ISiteService siteService,
            ISmtpService smtpService,
            IShapeFactory shapeFactory,
            IHtmlDisplay displayManager,
            IStringLocalizer<PasswordController> stringLocalizer)
        {
            _userService = userService;
            _siteService = siteService;
            _smtpService = smtpService;
            _shapeFactory = shapeFactory;
            _displayManager = displayManager;

            T = stringLocalizer;
        }

        IStringLocalizer T { get; set; }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<PasswordSettings>().EnableLostPassword)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<PasswordSettings>().EnableLostPassword)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = (User)await _userService.GetForgotPasswordUserAsync(model.UserIdentifier);
                if (user != null)
                {
                    user.ResetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ResetToken));
                    // send email with callback link
                    await SendResetTokenAsync(user);
                }
            }

            // returns to confirmation page anyway: we don't want to let scrapers know if a username or an email exist
            return RedirectToLocal(Url.Action("ForgotPasswordConfirmation", "Password"));
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
            if (!(await _siteService.GetSiteSettingsAsync()).As<PasswordSettings>().EnableLostPassword)
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
            if (!(await _siteService.GetSiteSettingsAsync()).As<PasswordSettings>().EnableLostPassword)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _userService.ResetPasswordAsync(model.Email, Encoding.UTF8.GetString(Convert.FromBase64String(model.ResetToken)), model.NewPassword, (key, message) => ModelState.AddModelError(key, message)))
                {
                    return RedirectToLocal(Url.Action("ResetPasswordConfirmation", "Password"));
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

        protected async Task<bool> SendResetTokenAsync(User user)
        {
            var viewName = "TemplateUserLostPassword";
            var model = Arguments.From(new { User = user, LostPasswordUrl = Url.Action("ResetPassword", "Password", new { code = user.ResetToken }, HttpContext.Request.Scheme) });

            var options = ControllerContext.HttpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();
            ControllerContext.RouteData.Values["action"] = viewName;
            ControllerContext.RouteData.Values["controller"] = "";
            var viewEngineResult = options.Value.ViewEngines.Select(x => x.FindView(ControllerContext, viewName, true)).FirstOrDefault(x => x != null);
            var displayContext = new DisplayContext()
            {
                ServiceProvider = ControllerContext.HttpContext.RequestServices,
                Value = await _shapeFactory.CreateAsync(viewName, model),
                ViewContext = new ViewContext(ControllerContext, viewEngineResult.View, ViewData, TempData, new StringWriter(), new HtmlHelperOptions())
            };
            var htmlContent = await _displayManager.ExecuteAsync(displayContext);

            var message = new MailMessage() { Body = WebUtility.HtmlDecode(htmlContent.ToString()), IsBodyHtml = true, Subject = T["Reset your password"] };
            message.To.Add(user.Email);

            // send email
            var result = await _smtpService.SendAsync(message);

            return result.Succeeded;
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