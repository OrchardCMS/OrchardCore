using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.Models;
using OrchardCore.Email.Services;
using OrchardCore.Email.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Email.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IShapeFactory New;
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
            New = shapeFactory;
            _smtpService = smtpService;
        }

        [HttpPost]
        public async Task<IActionResult> SmtpSavedSettingsTest()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Unauthorized();
            }

            var groupId = SmtpSettingsDisplayDriver.GroupId;

            // bind the model again for the test options
            var testSettings = new SmtpSettingsViewModel();
            if (await TryUpdateModelAsync(testSettings, nameof(ISite)))
            {
                // send eamil with DefaultSender
                var result = await _smtpService
                    .SendAsync(new EmailMessage
                    {
                        Recipients = testSettings.To,
                        Cc = testSettings.Cc,
                        Bcc = testSettings.Bcc,
                        ReplyTo = testSettings.ReplyTo,
                        Subject = testSettings.Subject,
                        Body = testSettings.Body,
                    });

                if (!result.Success)
                {
                    ModelState.AddModelError("*", result.ErrorMessage);
                }
                else
                {
                    _notifier.Success(H["Message sent!"]);
                }
            }

            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await CreateSmtpSettingsShape(testSettings);

            return View("/Packages/OrchardCore.Settings/Views/Admin/Index.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SmtpSettingsTest()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageEmailSettings))
            {
                return Unauthorized();
            }

            var groupId = SmtpSettingsDisplayDriver.GroupId;

            // bind the model for the test options
            var testSettings = new SmtpSettingsViewModel();
            if (await TryUpdateModelAsync(testSettings, nameof(ISite)))
            {
                // send email with DefaultSender
                var result = await _smtpService
                    .WithSettings(testSettings)
                    .SendAsync(new EmailMessage
                    {
                        Recipients = testSettings.To,
                        Cc = testSettings.Cc,
                        Bcc = testSettings.Bcc,
                        ReplyTo = testSettings.ReplyTo,
                        Subject = testSettings.Subject,
                        Body = testSettings.Body,
                    });

                if (!result.Success)
                {
                    ModelState.AddModelError("*", result.ErrorMessage);
                }
                else
                {
                    _notifier.Success(H["Message sent!"]);
                }
            }

            var viewModel = new AdminIndexViewModel();
            viewModel.GroupId = groupId;
            viewModel.Shape = await CreateSmtpSettingsShape(testSettings);

            return View("/Packages/OrchardCore.Settings/Views/Admin/Index.cshtml", viewModel);
        }


        private async Task<IShape> CreateSmtpSettingsShape(SmtpSettingsViewModel viewModel)
        {
            var shape = await New.CreateAsync<SmtpSettingsViewModel>("SmtpSettings_Edit", model =>
            {
                model.DefaultSender = viewModel.DefaultSender;
                model.Host = viewModel.Host;
                model.Port = viewModel.Port;
                model.EnableSsl = viewModel.EnableSsl;
                model.RequireCredentials = viewModel.RequireCredentials;
                model.UseDefaultCredentials = viewModel.UseDefaultCredentials;
                model.UserName = viewModel.UserName;
                model.Password = viewModel.Password;
                model.To = viewModel.To;
                model.Bcc = viewModel.Bcc;
                model.Cc = viewModel.Cc;
                model.ReplyTo = viewModel.ReplyTo;
                model.Subject = viewModel.Subject;
                model.Body = viewModel.Body;
            });

            shape.Metadata.Prefix = nameof(ISite);

            return shape;
        }

    }
}
