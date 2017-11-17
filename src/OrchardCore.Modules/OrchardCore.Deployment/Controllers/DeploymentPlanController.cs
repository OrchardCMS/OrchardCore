using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Deployment.Controllers
{
    [Admin]
    public class DeploymentPlanController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<DeploymentStep> _displayManager;
        private readonly IEnumerable<IDeploymentStepFactory> _factories;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        
        public DeploymentPlanController(
            IAuthorizationService authorizationService,
            IDisplayManager<DeploymentStep> displayManager,
            IEnumerable<IDeploymentStepFactory> factories,
            ISession session,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IStringLocalizer<DeploymentPlanController> stringLocalizer,
            IHtmlLocalizer<DeploymentPlanController> htmlLocalizer,
            INotifier notifier)
        {
            _displayManager = displayManager;
            _factories = factories;
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

            var deploymentPlans = _session.Query<DeploymentPlan, DeploymentPlanIndex>();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                deploymentPlans = deploymentPlans.Where(dp => dp.Name.Contains(options.Search));
            }

            var count = await deploymentPlans.CountAsync();

            var results = await deploymentPlans
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

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
                dynamic item = await _displayManager.BuildDisplayAsync(step, this, "Summary");
                item.DeploymentStep = step;
                items.Add(item);
            }

            var thumbnails = new Dictionary<string, dynamic>();
            foreach (var factory in _factories)
            {
                var step = factory.Create();
                dynamic thumbnail = await _displayManager.BuildDisplayAsync(step, this, "Thumbnail");
                thumbnail.DeploymentStep = step;
                thumbnails.Add(factory.Name, thumbnail);
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
