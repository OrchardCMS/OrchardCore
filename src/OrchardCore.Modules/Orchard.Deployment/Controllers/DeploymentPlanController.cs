using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.Admin;
using Orchard.Deployment.Editors;
using Orchard.Deployment.Indexes;
using Orchard.Deployment.ViewModels;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.Settings;
using YesSql;

namespace Orchard.Deployment.Controllers
{
    [Admin]
    public class DeploymentPlanController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDeploymentStepDisplayManager _displayManager;
        private readonly IEnumerable<IDeploymentStepDisplayDriver> _drivers;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        
        public DeploymentPlanController(
            IAuthorizationService authorizationService,
            IDeploymentStepDisplayManager displayManager,
            IEnumerable<IDeploymentStepDisplayDriver> drivers,
            ISession session,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IStringLocalizer<DeploymentPlanController> stringLocalizer,
            IHtmlLocalizer<DeploymentPlanController> htmlLocalizer,
            INotifier notifier)
        {
            _displayManager = displayManager;
            _drivers = drivers;
            _authorizationService = authorizationService;
            _session = session;
            _siteService = siteService;
            New = shapeFactory;
            _notifier = notifier;
            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public dynamic New { get; set; }

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(DeploymentPlanIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Export))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new DeploymentPlanIndexOptions();
            }

            var deploymentPlans = _session.QueryAsync<DeploymentPlan, DeploymentPlanIndex>();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                deploymentPlans = deploymentPlans.Where(dp => dp.Name.Contains(options.Search));
            }

            var count = await deploymentPlans.Count();

            var results = await deploymentPlans
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .List();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = New.Pager(pager).TotalItemCount(count).RouteData(routeData);

            var model = new DeploymentPlanIndexViewModel
            {
                DeploymentPlans = results.Select(x => new DeploymentPlanEntry { DeploymentPlan = x }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Display(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var items = new List<dynamic>();
            foreach (var step in deploymentPlan.DeploymentSteps)
            {
                var item = await _displayManager.DisplayStepAsync(step, this, "Summary");
                items.Add(item);
            }

            var thumbnails = new Dictionary<string, dynamic>();
            foreach (var driver in _drivers)
            {
                var thumbnail = await _displayManager.DisplayStepAsync(driver.Create(), this, "Thumbnail");
                thumbnails.Add(driver.Type.Name, thumbnail);
            }

            var model = new DisplayDeploymentPlanViewModel
            {
                DeploymentPlan = deploymentPlan,
                Items = items,
                Thumbnails = thumbnails,
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var model = new CreateDeploymentPlanViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDeploymentPlanViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(CreateDeploymentPlanViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var deploymentPlan = new DeploymentPlan { Name = model.Name };

                _session.Save(deploymentPlan);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

            if (deploymentPlan == null)
            { 
                return NotFound();
            }

            var model = new EditDeploymentPlanViewModel
            {
                Id = deploymentPlan.Id,
                Name = deploymentPlan.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDeploymentPlanViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(model.Id);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(EditDeploymentPlanViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                deploymentPlan.Name = model.Name;

                _session.Save(deploymentPlan);

                _notifier.Success(H["Deployment plan updated successfully"]);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            _session.Delete(deploymentPlan);

            _notifier.Success(H["Deployment plan deleted successfully"]);
            
            return RedirectToAction(nameof(Index));
        }
    }
}
