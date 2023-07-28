using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class RemoteInstanceController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly PagerOptions _pagerOptions;
        private readonly INotifier _notifier;
        private readonly RemoteInstanceService _service;

        protected readonly dynamic New;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;

        public RemoteInstanceController(
            RemoteInstanceService service,
            IAuthorizationService authorizationService,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            IStringLocalizer<RemoteInstanceController> stringLocalizer,
            IHtmlLocalizer<RemoteInstanceController> htmlLocalizer,
            INotifier notifier
            )
        {
            _authorizationService = authorizationService;
            _pagerOptions = pagerOptions.Value;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
            _notifier = notifier;
            _service = service;
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

            var remoteInstances = (await _service.GetRemoteInstanceListAsync()).RemoteInstances;

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                remoteInstances = remoteInstances.Where(x => x.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(remoteInstances.Count).RouteData(routeData);

            var model = new RemoteInstanceIndexViewModel
            {
                RemoteInstances = remoteInstances,
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
        public ActionResult IndexFilterPOST(RemoteInstanceIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Forbid();
            }

            var model = new EditRemoteInstanceViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditRemoteInstanceViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                await _service.CreateRemoteInstanceAsync(model.Name, model.Url, model.ClientName, model.ApiKey);

                await _notifier.SuccessAsync(H["Remote instance created successfully."]);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRemoteInstances))
            {
                return Forbid();
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
                return Forbid();
            }

            var remoteInstance = await _service.LoadRemoteInstanceAsync(model.Id);

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
                await _service.UpdateRemoteInstance(model.Id, model.Name, model.Url, model.ClientName, model.ApiKey);

                await _notifier.SuccessAsync(H["Remote instance updated successfully."]);

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
                return Forbid();
            }

            var remoteInstance = await _service.LoadRemoteInstanceAsync(id);

            if (remoteInstance == null)
            {
                return NotFound();
            }

            await _service.DeleteRemoteInstanceAsync(id);

            await _notifier.SuccessAsync(H["Remote instance deleted successfully."]);

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
                var remoteInstances = (await _service.LoadRemoteInstanceListAsync()).RemoteInstances;
                var checkedContentItems = remoteInstances.Where(x => itemIds.Contains(x.Id)).ToList();

                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems)
                        {
                            await _service.DeleteRemoteInstanceAsync(item.Id);
                        }
                        await _notifier.SuccessAsync(H["Remote instances successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }

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
                if (!Uri.TryCreate(model.Url, UriKind.Absolute, out _))
                {
                    ModelState.AddModelError(nameof(EditRemoteInstanceViewModel.Url), S["The url is invalid."]);
                }
            }
        }
    }
}
