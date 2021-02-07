using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Layers.Controllers
{
    [Admin]
    public class LayerRuleController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<Rule> _displayManager;
        private readonly IEnumerable<IRuleFactory> _factories;
        private readonly ILayerService _layerService;
        private readonly IRuleIdGenerator _ruleIdGenerator;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public LayerRuleController(
            IAuthorizationService authorizationService,
            IDisplayManager<Rule> displayManager,
            IEnumerable<IRuleFactory> factories,
            ILayerService layerService,
            IRuleIdGenerator ruleIdGenerator,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<LayerRuleController> htmlLocalizer,
            INotifier notifier,
            IUpdateModelAccessor updateModelAccessor)
        {
            _displayManager = displayManager;
            _factories = factories;
            _authorizationService = authorizationService;
            _layerService = layerService;
            _ruleIdGenerator = ruleIdGenerator;
            _siteService = siteService;
            _notifier = notifier;
            _updateModelAccessor = updateModelAccessor;
            New = shapeFactory;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Create(string queryType, string name, string type)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.GetLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, name));

            if (layer == null)
            {
                return NotFound();
            }

            var rule = _factories.FirstOrDefault(x => x.Name == type)?.Create();

            if (rule == null)
            {
                return NotFound();
            }

            // rule.RuleId = _ruleIdGenerator.GenerateUniqueId();

            var model = new EditLayerRuleViewModel
            {
                LayerName = name,
                // DeploymentStep = rule,
                // DeploymentStepId = rule.Id,
                RuleType = type,
                Editor = await _displayManager.BuildEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: true)
            };

            // model.Editor.Rule = rule;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditLayerRuleViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.GetLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, model.LayerName));

            if (layer == null)
            {
                return NotFound();
            }

            var rule = _factories.FirstOrDefault(x => x.Name == model.RuleType)?.Create();

            if (rule == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: true);
            // editor.DeploymentStep = rule;

            if (ModelState.IsValid)
            {
                rule.RuleId = _ruleIdGenerator.GenerateUniqueId();
                // TODO find nested rule, will need id, and add it...

                _notifier.Success(H["Rule added successfully."]);
                return RedirectToAction("Edit", "Admin", new { name = model.LayerName });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        public async Task<IActionResult> Edit(string name, string ruleId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.GetLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, name));

            if (layer == null)
            {
                return NotFound();
            }

            var rule = FindRule(layer.AllRule, ruleId);

            if (rule == null)
            {
                return NotFound();
            }

            var model = new EditLayerRuleViewModel
            {
                LayerName = name,
                // DeploymentStep = step,
                // DeploymentStepId = step.Id,
                // DeploymentStepType = rule.GetType().Name,
                Editor = await _displayManager.BuildEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: false)
            };

            // model.Editor.DeploymentStep = step;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditLayerRuleViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.GetLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, model.LayerName));

            if (layer == null)
            {
                return NotFound();
            }

            var rule = FindRule(layer.AllRule, model.RuleId);

            if (rule == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: false);

            if (ModelState.IsValid)
            {
                // This should be updated by reference here.

                await _layerService.UpdateAsync(layers);
                _notifier.Success(H["Rule updated successfully."]);
                return RedirectToAction("Edit", "Admin", new { name = model.LayerName });
            }

            _notifier.Error(H["The rule has validation errors."]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // TODO move to helper service.

        private Rule FindRule(Rule rule, string ruleId)
        {
            if (String.Equals(rule.RuleId, ruleId, StringComparison.OrdinalIgnoreCase))
            {
                return rule;
            }

            if (!(rule is GroupRule groupRule))
            {
                return null;
            }

            if (!groupRule.Children.Any())
            {
                return null;
            }

            Rule result;

            foreach (var nestedRule in groupRule.Children)
            {
                // Search in inner rules
                result = FindRule(rule, ruleId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }        
/*
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string stepId)
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

            _notifier.Success(H["Deployment step deleted successfully."]);

            return RedirectToAction("Display", "DeploymentPlan", new { id });
        }
        */
    }
}
