using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.Deployment.Remote.Controllers
{
    [Admin]
    public class RemoteClientController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IAuthorizationService _authorizationService;
        private readonly PagerOptions _pagerOptions;
        private readonly RemoteClientService _remoteClientService;
        private readonly INotifier _notifier;

        protected readonly dynamic New;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;

        public RemoteClientController(
            IDataProtectionProvider dataProtectionProvider,
            RemoteClientService remoteClientService,
            IAuthorizationService authorizationService,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            IStringLocalizer<RemoteClientController> stringLocalizer,
            IHtmlLocalizer<RemoteClientController> htmlLocalizer,
            INotifier notifier
            )
        {
            _authorizationService = authorizationService;
            _pagerOptions = pagerOptions.Value;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
            _notifier = notifier;
            _remoteClientService = remoteClientService;
            _dataProtector = dataProtectionProvider.CreateProtector("OrchardCore.Deployment").ToTimeLimitedDataProtector();
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteClients))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

            var remoteClients = (await _remoteClientService.GetRemoteClientListAsync()).RemoteClients;

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                remoteClients = remoteClients.Where(x => x.ClientName.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var count = remoteClients.Count;

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new RemoteClientIndexViewModel
            {
                RemoteClients = remoteClients,
                Pager = pagerShape,
                Options = options
            };

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(RemoteClientIndexViewModel model)
        {
            return RedirectToAction("Index", new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
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
                await _remoteClientService.CreateRemoteClientAsync(model.ClientName, model.ApiKey);

                await _notifier.SuccessAsync(H["Remote client created successfully."]);
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

            var remoteClient = await _remoteClientService.GetRemoteClientAsync(id);

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

            var remoteClient = await _remoteClientService.GetRemoteClientAsync(model.Id);

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
                await _remoteClientService.TryUpdateRemoteClient(model.Id, model.ClientName, model.ApiKey);

                await _notifier.SuccessAsync(H["Remote client updated successfully."]);

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

            var remoteClient = await _remoteClientService.GetRemoteClientAsync(id);

            if (remoteClient == null)
            {
                return NotFound();
            }

            await _remoteClientService.DeleteRemoteClientAsync(id);

            await _notifier.SuccessAsync(H["Remote client deleted successfully."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPost(ViewModels.ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var remoteClients = (await _remoteClientService.GetRemoteClientListAsync()).RemoteClients;
                var checkedContentItems = remoteClients.Where(x => itemIds.Contains(x.Id)).ToList();

                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems)
                        {
                            await _remoteClientService.DeleteRemoteClientAsync(item.Id);
                        }
                        await _notifier.SuccessAsync(H["Remote clients successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }

            return RedirectToAction("Index");
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
