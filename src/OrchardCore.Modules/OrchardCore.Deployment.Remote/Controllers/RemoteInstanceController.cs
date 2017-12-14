using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using YesSql;

namespace OrchardCore.Deployment.Remote.Controllers
{

    [Admin]
    public class RemoteInstanceController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly INotifier _notifier;
        private readonly RemoteInstanceService _service;

        public RemoteInstanceController(
            RemoteInstanceService service,
            IAuthorizationService authorizationService,
            ISession session,
            IStringLocalizer<RemoteInstanceController> stringLocalizer,
            IHtmlLocalizer<RemoteInstanceController> htmlLocalizer,
            INotifier notifier
            )
        {
            _session = session;
            _authorizationService = authorizationService;
            S = stringLocalizer;
            H = htmlLocalizer;
            _notifier = notifier;
            _service = service;
        }

        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Unauthorized();
            }

            var remoteInstanceList = await _service.GetRemoteInstanceListAsync();
            
            var model = new RemoteInstanceIndexViewModel
            {
                RemoteInstanceList = remoteInstanceList
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Unauthorized();
            }

            var model = new EditRemoteInstanceViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditRemoteInstanceViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                await _service.CreateRemoteInstanceAsync(model.Name, model.Url, model.ClientName, model.ApiKey);

                _notifier.Success(H["Remote instance created successfully"]);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Unauthorized();
            }

            var remoteInstance = await _service.GetRemoteInstanceAsync(id);

            if (remoteInstance == null)
            {
                return NotFound();
            }

            var model = new EditRemoteInstanceViewModel
            {
                Id = remoteInstance.Id,
                Name = remoteInstance.Name,
                ClientName = remoteInstance.ClientName,
                ApiKey = remoteInstance.ApiKey,
                Url = remoteInstance.Url
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditRemoteInstanceViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Unauthorized();
            }

            var remoteInstance = await _service.GetRemoteInstanceAsync(model.Id);

            if (remoteInstance == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                await _service.TryUpdateRemoteInstance(model.Id, model.Name, model.Url, model.ClientName, model.ApiKey);

                _notifier.Success(H["Remote instance updated successfully"]);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Unauthorized();
            }

            var remoteInstance = await _service.GetRemoteInstanceAsync(id);

            if (remoteInstance == null)
            {
                return NotFound();
            }

            await _service.DeleteRemoteInstanceAsync(id);

            _notifier.Success(H["Remote instance deleted successfully"]);

            return RedirectToAction(nameof(Index));
        }

        private void ValidateViewModel(EditRemoteInstanceViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(EditRemoteInstanceViewModel.Name), S["The name is mandatory."]);
            }

            if (String.IsNullOrWhiteSpace(model.ClientName))
            {
                ModelState.AddModelError(nameof(EditRemoteInstanceViewModel.ClientName), S["The client name is mandatory."]);
            }

            if (String.IsNullOrWhiteSpace(model.ApiKey))
            {
                ModelState.AddModelError(nameof(EditRemoteInstanceViewModel.ApiKey), S["The api key is mandatory."]);
            }

            if (String.IsNullOrWhiteSpace(model.Url))
            {
                ModelState.AddModelError(nameof(EditRemoteInstanceViewModel.Url), S["The url is mandatory."]);
            }
            else
            {
                Uri uri;
                if (!Uri.TryCreate(model.Url, UriKind.Absolute, out uri))
                {
                    ModelState.AddModelError(nameof(EditRemoteInstanceViewModel.Url), S["The url is invalid."]);
                }
            }
        }
    }
}
