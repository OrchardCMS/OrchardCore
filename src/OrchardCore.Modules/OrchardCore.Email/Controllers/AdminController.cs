using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly ISmtpService _smtpService;
        protected readonly IHtmlLocalizer H;

        public AdminController(
            IHtmlLocalizer<AdminController> h,
            IAuthorizationService authorizationService,
            INotifier notifier,
            ISmtpService smtpService)
        {
            H = h;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _smtpService = smtpService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Forbid();
            }

            return View();
        }

        [HttpPost, ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(SmtpSettingsViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var result = await _smtpService.SendAsync(model.To, model.Cc, model.Bcc, model.ReplyTo, model.Subject, model.Body, isHtmlBody: false);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("*", error.ToString());
                    }
                }
                else
                {
                    await _notifier.SuccessAsync(H["Message sent successfully."]);

                    return Redirect(Url.Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmtpSettingsDisplayDriver.GroupId }));
                }
            }

            return View(model);
        }
    }
}
