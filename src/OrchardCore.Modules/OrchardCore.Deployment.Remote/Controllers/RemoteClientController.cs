using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Deployment.Remote.Controllers
{
    [Admin]
    public class RemoteClientController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly RemoteClientService _service;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public RemoteClientController(
            IDataProtectionProvider dataProtectionProvider,
            RemoteClientService service,
            IAuthorizationService authorizationService,
            IStringLocalizer<RemoteClientController> stringLocalizer,
            IHtmlLocalizer<RemoteClientController> htmlLocalizer,
            INotifier notifier
            )
        {
            _authorizationService = authorizationService;
            S = stringLocalizer;
            H = htmlLocalizer;
            _notifier = notifier;
            _service = service;
            _dataProtector = dataProtectionProvider.CreateProtector("OrchardCore.Deployment").ToTimeLimitedDataProtector();
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            var remoteClientList = await _service.GetRemoteClientListAsync();

            var model = new RemoteClientIndexViewModel
            {
                RemoteClientList = remoteClientList
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            var model = new EditRemoteClientViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditRemoteClientViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                await _service.CreateRemoteClientAsync(model.ClientName, model.ApiKey);

                _notifier.Success(H["Remote client created successfully"]);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            var remoteClient = await _service.GetRemoteClientAsync(id);

            if (remoteClient == null)
            {
                return NotFound();
            }

            var model = new EditRemoteClientViewModel
            {
                Id = remoteClient.Id,
                ClientName = remoteClient.ClientName,
                ApiKey = Encoding.UTF8.GetString(_dataProtector.Unprotect(remoteClient.ProtectedApiKey)),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditRemoteClientViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            var remoteClient = await _service.GetRemoteClientAsync(model.Id);

            if (remoteClient == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                await _service.TryUpdateRemoteClient(model.Id, model.ClientName, model.ApiKey);

                _notifier.Success(H["Remote client updated successfully"]);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            var remoteClient = await _service.GetRemoteClientAsync(id);

            if (remoteClient == null)
            {
                return NotFound();
            }

            await _service.DeleteRemoteClientAsync(id);

            _notifier.Success(H["Remote client deleted successfully"]);

            return RedirectToAction(nameof(Index));
        }

        private void ValidateViewModel(EditRemoteClientViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.ClientName))
            {
                ModelState.AddModelError(nameof(EditRemoteClientViewModel.ClientName), S["The client name is mandatory."]);
            }

            if (String.IsNullOrWhiteSpace(model.ApiKey))
            {
                ModelState.AddModelError(nameof(EditRemoteClientViewModel.ApiKey), S["The api key is mandatory."]);
            }
        }
    }
}
