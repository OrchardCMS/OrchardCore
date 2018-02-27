using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly ISmtpService _smtpService;
        private readonly IHtmlLocalizer H;

        public AdminController(
            IHtmlLocalizer<AdminController> h,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IShapeFactory shapeFactory,
            ISmtpService smtpService)
        {
            H = h;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _smtpService = smtpService;
        }

        [HttpGet]
        [ActionName("Index")]
        public async Task<IActionResult> Get()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> Post(SmtpSettingsViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var message = CreateMessageFromViewModel(model);

                // send email with DefaultSender
                var result = await _smtpService.SendAsync(message);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("*", error.ToString());
                    }
                }
                else
                {
                    _notifier.Success(H["Message sent successfully"]);

                    return Redirect(Url.Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmtpSettingsDisplayDriver.GroupId }));
                }
            }

            return View(model);
        }

        private MailMessage CreateMessageFromViewModel(SmtpSettingsViewModel testSettings)
        {
            var message = new MailMessage();

            message.To.Add(testSettings.To);

            foreach (var address in ParseMailAddresses(testSettings.Cc))
            {
                message.CC.Add(address);
            }

            foreach (var address in ParseMailAddresses(testSettings.Bcc))
            {
                message.Bcc.Add(address);
            }

            foreach (var address in ParseMailAddresses(testSettings.ReplyTo))
            {
                message.ReplyToList.Add(address);
            }

            if (!String.IsNullOrWhiteSpace(testSettings.Subject))
            {
                message.Subject = testSettings.Subject;
            }

            if (!String.IsNullOrWhiteSpace(testSettings.Body))
            {
                message.Body = testSettings.Body;
            }

            return message;
        }

        private IEnumerable<string> ParseMailAddresses(string adresses)
        {
            if (String.IsNullOrWhiteSpace(adresses))
            {
                return Array.Empty<string>();
            }

            return adresses.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
