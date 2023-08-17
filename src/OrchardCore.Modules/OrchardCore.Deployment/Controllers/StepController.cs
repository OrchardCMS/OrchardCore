using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using YesSql;

namespace OrchardCore.Deployment.Controllers
{
    [Admin]
    public class StepController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<DeploymentStep> _displayManager;
        private readonly IEnumerable<IDeploymentStepFactory> _factories;
        private readonly ISession _session;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        protected readonly IHtmlLocalizer H;
        protected readonly dynamic New;

        public StepController(
            IAuthorizationService authorizationService,
            IDisplayManager<DeploymentStep> displayManager,
            IEnumerable<IDeploymentStepFactory> factories,
            ISession session,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<StepController> htmlLocalizer,
            INotifier notifier,
            IUpdateModelAccessor updateModelAccessor)
        {
            _displayManager = displayManager;
            _factories = factories;
            _authorizationService = authorizationService;
            _session = session;
            _notifier = notifier;
            _updateModelAccessor = updateModelAccessor;
            New = shapeFactory;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Create(long id, string type)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Forbid();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var step = _factories.FirstOrDefault(x => x.Name == type)?.Create();

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
                Editor = await _displayManager.BuildEditorAsync(step, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "")
            };

            model.Editor.DeploymentStep = step;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditDeploymentPlanStepViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Forbid();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(model.DeploymentPlanId);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var step = _factories.FirstOrDefault(x => x.Name == model.DeploymentStepType)?.Create();

            if (step == null)
            {
                return NotFound();
            }

            dynamic editor = await _displayManager.UpdateEditorAsync(step, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "");
            editor.DeploymentStep = step;

            if (ModelState.IsValid)
            {
                step.Id = model.DeploymentStepId;
                deploymentPlan.DeploymentSteps.Add(step);
                _session.Save(deploymentPlan);

                await _notifier.SuccessAsync(H["Deployment plan step added successfully."]);
                return RedirectToAction("Display", "DeploymentPlan", new { id = model.DeploymentPlanId });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(long id, string stepId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Forbid();
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
                Editor = await _displayManager.BuildEditorAsync(step, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "")
            };

            model.Editor.DeploymentStep = step;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDeploymentPlanStepViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Forbid();
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

            var editor = await _displayManager.UpdateEditorAsync(step, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");

            if (ModelState.IsValid)
            {
                _session.Save(deploymentPlan);

                await _notifier.SuccessAsync(H["Deployment plan step updated successfully."]);
                return RedirectToAction("Display", "DeploymentPlan", new { id = model.DeploymentPlanId });
            }

            await _notifier.ErrorAsync(H["The deployment plan step has validation errors."]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id, string stepId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Forbid();
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

            await _notifier.SuccessAsync(H["Deployment step deleted successfully."]);

            return RedirectToAction("Display", "DeploymentPlan", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(long id, int oldIndex, int newIndex)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDeploymentPlan))
            {
                return Forbid();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var step = deploymentPlan.DeploymentSteps.ElementAtOrDefault(oldIndex);

            if (step == null)
            {
                return NotFound();
            }

            deploymentPlan.DeploymentSteps.RemoveAt(oldIndex);

            deploymentPlan.DeploymentSteps.Insert(newIndex, step);

            _session.Save(deploymentPlan);

            return Ok();
        }
    }
}
