using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Feature("OrchardCore.Users.ChangeEmail")]
    public class ChangeEmailController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<IUser> _userManager;
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public ChangeEmailController(
            IUserService userService,
            UserManager<IUser> userManager,
            ISiteService siteService,
            IStringLocalizer<ChangeEmailController> stringLocalizer)
        {
            _userService = userService;
            _userManager = userManager;
            _siteService = siteService;

            S = stringLocalizer;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ChangeEmailSettings>().AllowChangeEmail)
            {
                return NotFound();
            }

            var user = await _userService.GetAuthenticatedUserAsync(User);

            return View(new ChangeEmailViewModel() { Email = ((User)user).Email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ChangeEmailViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ChangeEmailSettings>().AllowChangeEmail)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userService.GetAuthenticatedUserAsync(User);
                var userWithEmail = await _userManager.FindByEmailAsync(model.Email);

                if (((User)user).Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Email", S["This email is already your current one."]);
                }
                else if (userWithEmail != null && user.UserName != userWithEmail.UserName)
                {
                    ModelState.AddModelError("Email", S["A user with the same email already exists."]);
                }
                else
                {
                    if (await _userService.ChangeEmailAsync(user, model.Email,
                        (key, message) => ModelState.AddModelError(key, message)))
                    {
                        return RedirectToLocal(Url.Action("ChangeEmailConfirmation", "ChangeEmail"));
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangeEmailConfirmation()
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
