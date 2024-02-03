using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly EmailProviderOptions _providerOptions;
        private readonly IEmailProviderResolver _providerResolver;

        protected readonly IHtmlLocalizer H;
        protected readonly IStringLocalizer S;

        public AdminController(
            IAuthorizationService authorizationService,
            INotifier notifier,
            IOptions<EmailProviderOptions> providerOptions,
            IEmailProviderResolver providerResolver,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _providerOptions = providerOptions.Value;
            _providerResolver = providerResolver;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public async Task<IActionResult> Test()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Forbid();
            }

            var model = new EmailTestViewModel();

            PopulateModel(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Test(EmailTestViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Forbid();
            }

            var provider = await _providerResolver.GetAsync(model.Provider);

            if (provider is null)
            {
                ModelState.AddModelError(nameof(model.Provider), S["Please select a valid provider."]);
            }

            if (ModelState.IsValid)
            {
                var message = GetMessage(model);

                var result = await provider.SendAsync(message);

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

                    return Redirect(Url.Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = EmailSettings.GroupId }));
                }
            }

            return View(model);
        }

        private static MailMessage GetMessage(EmailTestViewModel testSettings)
        {
            var message = new MailMessage
            {
                To = testSettings.To,
                Bcc = testSettings.Bcc,
                Cc = testSettings.Cc,
                ReplyTo = testSettings.ReplyTo
            };

            if (!string.IsNullOrWhiteSpace(testSettings.Sender))
            {
                message.Sender = testSettings.Sender;
            }

            if (!string.IsNullOrWhiteSpace(testSettings.Subject))
            {
                message.Subject = testSettings.Subject;
            }

            if (!string.IsNullOrWhiteSpace(testSettings.Body))
            {
                message.Body = testSettings.Body;
            }

            return message;
        }

        private void PopulateModel(EmailTestViewModel model)
        {
            model.Providers = _providerOptions.Providers
                .Where(entry => entry.Value.IsEnabled)
                .Select(entry => new SelectListItem(entry.Key, entry.Key))
                .OrderBy(item => item.Text)
                .ToArray();
        }
    }
}
