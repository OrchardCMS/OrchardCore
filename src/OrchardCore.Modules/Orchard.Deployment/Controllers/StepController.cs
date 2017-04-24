using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Orchard.Admin;
using Orchard.Deployment.Editors;
using Orchard.Deployment.ViewModels;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Settings;
using YesSql;

namespace Orchard.Deployment.Controllers
{
    [Admin]
    public class StepController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDeploymentStepDisplayManager _displayManager;
        private readonly IEnumerable<IDeploymentStepDisplayDriver> _drivers;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public StepController(
            IAuthorizationService authorizationService,
            IDeploymentStepDisplayManager displayManager,
            IEnumerable<IDeploymentStepDisplayDriver> drivers,
            ISession session,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IStringLocalizer<StepController> stringLocalizer,
            IHtmlLocalizer<StepController> htmlLocalizer,
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

        public async Task<IActionResult> Create(int id, string type)
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

            var step = _drivers.FirstOrDefault(x => x.Type.Name == type)?.Create();

            if (step == null)
            {
                return NotFound();
            }

            step.Id = Guid.NewGuid().ToString("n");

            var model = new EditDeploymentPlanStepViewModel
            {
                DeploymentPlanId = id,
                DeploymentStep = step,
                DeploymentStepId = step.Id,
                DeploymentStepType = type,
                Editor = await _displayManager.BuildStepEditorAsync(step, this)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditDeploymentPlanStepViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(model.DeploymentPlanId);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var step = _drivers.FirstOrDefault(x => x.Type.Name == model.DeploymentStepType)?.Create();

            if (step == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdatStepEditorAsync(step, this);

            if (ModelState.IsValid)
            {
                step.Id = model.DeploymentStepId;
                deploymentPlan.DeploymentSteps.Add(step);
                _session.Save(deploymentPlan);

                _notifier.Success(H["Deployment plan step added successfully"]);
                return RedirectToAction("Display", "DeploymentPlan", new { id = model.DeploymentPlanId });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(int id, string stepId)
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

            var step = deploymentPlan.DeploymentSteps.FirstOrDefault(x => String.Equals(x.Id, stepId, StringComparison.OrdinalIgnoreCase));

            if (step == null)
            {
                return NotFound();
            }

            var model = new EditDeploymentPlanStepViewModel
            {
                DeploymentPlanId = id,
                DeploymentStep = step,
                DeploymentStepId = step.Id,
                DeploymentStepType = step.GetType().Name,
                Editor = await _displayManager.BuildStepEditorAsync(step, this)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDeploymentPlanStepViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Unauthorized();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(model.DeploymentPlanId);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var step = deploymentPlan.DeploymentSteps.FirstOrDefault(x => String.Equals(x.Id, model.DeploymentStepId, StringComparison.OrdinalIgnoreCase));

            if (step == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdatStepEditorAsync(step, this);

            if (ModelState.IsValid)
            {
                _session.Save(deploymentPlan);

                _notifier.Success(H["Deployment plan step updated successfully"]);
                return RedirectToAction("Display", "DeploymentPlan", new { id = model.DeploymentPlanId });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string stepId)
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

            var step = deploymentPlan.DeploymentSteps.FirstOrDefault(x => String.Equals(x.Id, stepId, StringComparison.OrdinalIgnoreCase));

            if (step == null)
            {
                return NotFound();
            }

            deploymentPlan.DeploymentSteps.Remove(step);
            _session.Save(deploymentPlan);

            _notifier.Success(H["Deployment step deleted successfully"]);

            return RedirectToAction("Display", "DeploymentPlan", new { id });
        }
    }
}
