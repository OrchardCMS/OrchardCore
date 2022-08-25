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
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Rules;

namespace OrchardCore.Layers.Controllers
{
    [Admin]
    public class LayerRuleController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<Condition> _displayManager;
        private readonly IEnumerable<IConditionFactory> _factories;
        private readonly ILayerService _layerService;
        private readonly IConditionIdGenerator _conditionIdGenerator;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public LayerRuleController(
            IAuthorizationService authorizationService,
            IDisplayManager<Condition> displayManager,
            IEnumerable<IConditionFactory> factories,
            ILayerService layerService,
            IConditionIdGenerator conditionIdGenerator,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<LayerRuleController> htmlLocalizer,
            INotifier notifier,
            IUpdateModelAccessor updateModelAccessor)
        {
            _displayManager = displayManager;
            _factories = factories;
            _authorizationService = authorizationService;
            _layerService = layerService;
            _conditionIdGenerator = conditionIdGenerator;
            _notifier = notifier;
            _updateModelAccessor = updateModelAccessor;
            New = shapeFactory;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Create(string queryType, string name, string type, string conditionGroupId)
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

            var conditionGroup = FindConditionGroup(layer.LayerRule, conditionGroupId);

            if (conditionGroup == null)
            {
                return NotFound();
            }

            var condition = _factories.FirstOrDefault(x => x.Name == type)?.Create();

            if (condition == null)
            {
                return NotFound();
            }

            var model = new LayerRuleCreateViewModel
            {
                Name = name,
                ConditionGroupId = conditionGroup.ConditionId,
                ConditionType = type,
                Editor = await _displayManager.BuildEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: true)
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

            var conditionGroup = FindConditionGroup(layer.LayerRule, model.ConditionGroupId);

            if (conditionGroup == null)
            {
                return NotFound();
            }

            var condition = _factories.FirstOrDefault(x => x.Name == model.ConditionType)?.Create();

            if (condition == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: true);

            if (ModelState.IsValid)
            {
                _conditionIdGenerator.GenerateUniqueId(condition);
                conditionGroup.Conditions.Add(condition);
                await _layerService.UpdateAsync(layers);
                await _notifier.SuccessAsync(H["Condition added successfully."]);
                return RedirectToAction(nameof(Edit), "Admin", new { name = model.Name });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name, string conditionId)
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

            var condition = FindCondition(layer.LayerRule, conditionId);

            if (condition == null)
            {
                return NotFound();
            }

            var model = new LayerRuleEditViewModel
            {
                Name = name,
                Editor = await _displayManager.BuildEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: false)
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

            var condition = FindCondition(layer.LayerRule, model.ConditionId);

            if (condition == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: false);

            if (ModelState.IsValid)
            {
                await _layerService.UpdateAsync(layers);
                await _notifier.SuccessAsync(H["Condition updated successfully."]);
                return RedirectToAction(nameof(Edit), "Admin", new { name = model.Name });
            }

            await _notifier.ErrorAsync(H["The condition has validation errors."]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name, string conditionId)
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

            var condition = FindCondition(layer.LayerRule, conditionId);
            var conditionParent = FindConditionParent(layer.LayerRule, conditionId);

            if (condition == null || conditionParent == null)
            {
                return NotFound();
            }

            conditionParent.Conditions.Remove(condition);
            await _layerService.UpdateAsync(layers);

            await _notifier.SuccessAsync(H["Condition deleted successfully."]);

            return RedirectToAction(nameof(Edit), "Admin", new { name = name });
        }

        public async Task<IActionResult> UpdateOrder(string name, string conditionId, string toConditionId, int toPosition)
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

            var condition = FindCondition(layer.LayerRule, conditionId);
            var conditionParent = FindConditionParent(layer.LayerRule, conditionId);
            var toCondition = FindCondition(layer.LayerRule, toConditionId);

            if (condition == null || conditionParent == null || toCondition == null || !(toCondition is ConditionGroup toGroupCondition))
            {
                return NotFound();
            }

            conditionParent.Conditions.Remove(condition);
            toGroupCondition.Conditions.Insert(toPosition, condition);

            await _layerService.UpdateAsync(layers);

            return Ok();
        }

        private Condition FindCondition(Condition condition, string conditionId)
        {
            if (String.Equals(condition.ConditionId, conditionId, StringComparison.OrdinalIgnoreCase))
            {
                return condition;
            }

            if (!(condition is ConditionGroup conditionGroup))
            {
                return null;
            }

            if (!conditionGroup.Conditions.Any())
            {
                return null;
            }

            Condition result;

            foreach (var nestedCondition in conditionGroup.Conditions)
            {
                // Search in inner conditions
                result = FindCondition(nestedCondition, conditionId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private ConditionGroup FindConditionGroup(ConditionGroup condition, string groupconditionId)
        {
            if (String.Equals(condition.ConditionId, groupconditionId, StringComparison.OrdinalIgnoreCase))
            {
                return condition;
            }

            if (!condition.Conditions.Any())
            {
                return null;
            }

            ConditionGroup result;

            foreach (var nestedCondition in condition.Conditions.OfType<ConditionGroup>())
            {
                // Search in inner conditions
                result = FindConditionGroup(nestedCondition, groupconditionId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private ConditionGroup FindConditionParent(ConditionGroup condition, string conditionId)
        {
            ConditionGroup result;

            foreach (var nestedCondition in condition.Conditions)
            {
                if (String.Equals(nestedCondition.ConditionId, conditionId, StringComparison.OrdinalIgnoreCase))
                {
                    return condition;
                }

                if (nestedCondition is ConditionGroup nestedConditionGroup)
                {
                    result = FindConditionParent(nestedConditionGroup, conditionId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
