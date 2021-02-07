using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Settings;

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

        public async Task<IActionResult> Create(string queryType, string name, string type, string ruleGroupId)
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

            var ruleGroup = FindRuleGroup(layer.RuleContainer, ruleGroupId);

            if (ruleGroup == null)
            {
                return NotFound();
            }

            var rule = _factories.FirstOrDefault(x => x.Name == type)?.Create();

            if (rule == null)
            {
                return NotFound();
            }

            var model = new LayerRuleCreateViewModel
            {
                Name = name,
                RuleGroupId = ruleGroup.RuleId,
                RuleType = type,
                Editor = await _displayManager.BuildEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: true)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LayerRuleCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.LoadLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, model.Name));

            if (layer == null)
            {
                return NotFound();
            }

            var ruleGroup = FindRuleGroup(layer.RuleContainer, model.RuleGroupId);

            if (ruleGroup == null)
            {
                return NotFound();
            }

            var rule = _factories.FirstOrDefault(x => x.Name == model.RuleType)?.Create();

            if (rule == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: true);

            if (ModelState.IsValid)
            {
                rule.RuleId = _ruleIdGenerator.GenerateUniqueId();
                ruleGroup.Rules.Add(rule);
                await _layerService.UpdateAsync(layers);
                _notifier.Success(H["Rule added successfully."]);
                return RedirectToAction("Edit", "Admin", new { name = model.Name });
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

            var rule = FindRule(layer.RuleContainer, ruleId);

            if (rule == null)
            {
                return NotFound();
            }

            var model = new LayerRuleEditViewModel
            {
                Name = name,
                Editor = await _displayManager.BuildEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: false)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(LayerRuleEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.LoadLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, model.Name));

            if (layer == null)
            {
                return NotFound();
            }

            var rule = FindRule(layer.RuleContainer, model.RuleId);

            if (rule == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: false);

            if (ModelState.IsValid)
            {
                await _layerService.UpdateAsync(layers);
                _notifier.Success(H["Rule updated successfully."]);
                return RedirectToAction("Edit", "Admin", new { name = model.Name });
            }

            _notifier.Error(H["The rule has validation errors."]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private Rule FindRule(Rule rule, string ruleId)
        {
            if (String.Equals(rule.RuleId, ruleId, StringComparison.OrdinalIgnoreCase))
            {
                return rule;
            }

            if (!(rule is RuleGroup groupRule))
            {
                return null;
            }

            if (!groupRule.Rules.Any())
            {
                return null;
            }

            Rule result;

            foreach (var nestedRule in groupRule.Rules)
            {
                // Search in inner rules
                result = FindRule(nestedRule, ruleId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private RuleGroup FindRuleGroup(RuleGroup rule, string groupRuleId)
        {
            if (String.Equals(rule.RuleId, groupRuleId, StringComparison.OrdinalIgnoreCase))
            {
                return rule;
            }

            if (!rule.Rules.Any())
            {
                return null;
            }

            RuleGroup result;

            foreach (var nestedRule in rule.Rules.OfType<RuleGroup>())
            {
                // Search in inner rules
                result = FindRuleGroup(nestedRule, groupRuleId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        private RuleGroup FindRuleParent(RuleGroup rule, string ruleId)
        {
            RuleGroup result;

            foreach (var nestedRule in rule.Rules)
            {
                if (String.Equals(nestedRule.RuleId, ruleId, StringComparison.OrdinalIgnoreCase))
                {
                    return rule;
                }

                if (nestedRule is RuleGroup nestedRuleGroup)
                {
                    result = FindRuleParent(nestedRuleGroup, ruleId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name, string ruleId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
            {
                return Forbid();
            }

            var layers = await _layerService.LoadLayersAsync();
            var layer = layers.Layers.FirstOrDefault(x => String.Equals(x.Name, name));

            if (layer == null)
            {
                return NotFound();
            }

            var rule = FindRule(layer.RuleContainer, ruleId);

            if (rule == null)
            {
                return NotFound();
            }

            var ruleParent = FindRuleParent(layer.RuleContainer, ruleId);

            if (ruleParent == null)
            {
                return NotFound();
            }

            ruleParent.Rules.Remove(rule);
            await _layerService.UpdateAsync(layers);

            _notifier.Success(H["Rule deleted successfully."]);

            return RedirectToAction("Edit", "Admin", new { name = name });
        }
    }
}
