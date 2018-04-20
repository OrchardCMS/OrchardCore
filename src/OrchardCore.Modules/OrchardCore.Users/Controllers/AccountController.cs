using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

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
        private readonly ISmtpService _smtpService;
        private readonly ICompositeViewEngine _viewEngine;
        
        public AccountController(
            IUserService userService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<AccountController> logger,
            ISiteService siteService,
            ISmtpService smtpService,
            ICompositeViewEngine viewEngine,
            IStringLocalizer<AccountController> stringLocalizer,
            IHtmlLocalizer<AccountController> htmlLocalizer)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            _siteService = siteService;
            _smtpService = smtpService;
            _viewEngine = viewEngine;

            T = stringLocalizer;
            H = htmlLocalizer;
        }

        IStringLocalizer T { get; set; }
        IHtmlLocalizer H { get; set; }

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
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

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
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userService.CreateUserAsync(model.UserName, model.Email, new string[0], model.Password, (key, message) => ModelState.AddModelError(key, message));

                if (user != null)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                    //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/LogOff
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().EnableLostPassword)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().EnableLostPassword)
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
            return RedirectToLocal(Url.Action("ForgotPasswordConfirmation"));
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
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().EnableLostPassword)
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
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().EnableLostPassword)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _userService.ResetPasswordAsync(model.Email, Encoding.UTF8.GetString(Convert.FromBase64String(model.ResetToken)), model.NewPassword, (key, message) => ModelState.AddModelError(key, message)))
                {
                    return RedirectToLocal(Url.Action("ResetPasswordConfirmation"));
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
            var message = new MailMessage();
            message.IsBodyHtml = true;
            message.To.Add(user.Email);
            message.Subject = T["Reset your password"];

            LostPasswordViewModel model = new LostPasswordViewModel()
            {
                User = user,
                LostPasswordUrl = Url.Action("ResetPassword", "Account", new { code = user.ResetToken }, HttpContext.Request.Scheme)
            };
            // Todo: Find a way to render a shape
            string template = await RenderViewAsString("Template.User.LostPassword", model);
            message.Body = template;

            // send email
            var result = await _smtpService.SendAsync(message);

            return result.Succeeded;
        }

        protected async Task<string> RenderViewAsString(string viewName, object model)
        {
            viewName = viewName ?? ControllerContext.ActionDescriptor.ActionName;
            ViewData.Model = model;

            var viewEngineResult = _viewEngine.FindView(ControllerContext, viewName, false);
            var view = viewEngineResult.View;
            var viewDataDictionnary = new ViewDataDictionary(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            };

            using (StringWriter sw = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(ControllerContext, view, viewDataDictionnary, TempData, sw, new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return WebUtility.HtmlDecode(sw.ToString());
            }
        }
    }
}