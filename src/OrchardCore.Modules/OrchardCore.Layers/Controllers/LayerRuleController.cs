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
using OrchardCore.Rules.Services;

namespace OrchardCore.Layers.Controllers;

[Admin("Layers/Rules/{action}", "Layers.Rules.{action}")]
public sealed class LayerRuleController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<Condition> _displayManager;
    private readonly IEnumerable<IConditionFactory> _factories;
    private readonly ILayerService _layerService;
    private readonly IConditionIdGenerator _conditionIdGenerator;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IHtmlLocalizer H;

    public LayerRuleController(
        IAuthorizationService authorizationService,
        IDisplayManager<Condition> displayManager,
        IEnumerable<IConditionFactory> factories,
        ILayerService layerService,
        IConditionIdGenerator conditionIdGenerator,
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
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Create(string name, string type, string conditionGroupId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        var layers = await _layerService.GetLayersAsync();
        var layer = layers.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));

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
            Editor = await _displayManager.BuildEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: true, string.Empty, string.Empty)
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
        var layer = layers.Layers.FirstOrDefault(x => string.Equals(x.Name, model.Name, StringComparison.Ordinal));

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

        var editor = await _displayManager.UpdateEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "");

        if (ModelState.IsValid)
        {
            _conditionIdGenerator.GenerateUniqueId(condition);
            conditionGroup.Conditions.Add(condition);
            await _layerService.UpdateAsync(layers);
            await _notifier.SuccessAsync(H["Condition added successfully."]);
            return RedirectToAction(nameof(Edit), "Admin", new { name = model.Name });
        }

        model.Editor = editor;

        // If we got this far, something failed, redisplay form.
        return View(model);
    }

    public async Task<IActionResult> Edit(string name, string conditionId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        var layers = await _layerService.GetLayersAsync();
        var layer = layers.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));

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
            Editor = await _displayManager.BuildEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "")
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
        var layer = layers.Layers.FirstOrDefault(x => string.Equals(x.Name, model.Name, StringComparison.Ordinal));

        if (layer == null)
        {
            return NotFound();
        }

        var condition = FindCondition(layer.LayerRule, model.ConditionId);

        if (condition == null)
        {
            return NotFound();
        }

        var editor = await _displayManager.UpdateEditorAsync(condition, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");

        if (ModelState.IsValid)
        {
            await _layerService.UpdateAsync(layers);
            await _notifier.SuccessAsync(H["Condition updated successfully."]);
            return RedirectToAction(nameof(Edit), "Admin", new { name = model.Name });
        }

        await _notifier.ErrorAsync(H["The condition has validation errors."]);
        model.Editor = editor;

        // If we got this far, something failed, redisplay form.
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
        var layer = layers.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));

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

        return RedirectToAction(nameof(Edit), "Admin", new { name });
    }

    [Admin("Layers/Rules/Order", "Layers.Rules.Order")]
    public async Task<IActionResult> UpdateOrder(string name, string conditionId, string toConditionId, int toPosition)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        var layers = await _layerService.LoadLayersAsync();
        var layer = layers.Layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));

        if (layer == null)
        {
            return NotFound();
        }

        var condition = FindCondition(layer.LayerRule, conditionId);
        var conditionParent = FindConditionParent(layer.LayerRule, conditionId);
        var toCondition = FindCondition(layer.LayerRule, toConditionId);

        if (condition == null || conditionParent == null || toCondition == null || toCondition is not ConditionGroup toGroupCondition)
        {
            return NotFound();
        }

        conditionParent.Conditions.Remove(condition);
        toGroupCondition.Conditions.Insert(toPosition, condition);

        await _layerService.UpdateAsync(layers);

        return Ok();
    }

    private static Condition FindCondition(Condition condition, string conditionId)
    {
        if (string.Equals(condition.ConditionId, conditionId, StringComparison.OrdinalIgnoreCase))
        {
            return condition;
        }

        if (condition is not ConditionGroup conditionGroup)
        {
            return null;
        }

        if (conditionGroup.Conditions.Count == 0)
        {
            return null;
        }

        Condition result;

        foreach (var nestedCondition in conditionGroup.Conditions)
        {
            // Search in inner conditions.
            result = FindCondition(nestedCondition, conditionId);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static ConditionGroup FindConditionGroup(ConditionGroup condition, string groupConditionId)
    {
        if (string.Equals(condition.ConditionId, groupConditionId, StringComparison.OrdinalIgnoreCase))
        {
            return condition;
        }

        if (condition.Conditions.Count == 0)
        {
            return null;
        }

        ConditionGroup result;

        foreach (var nestedCondition in condition.Conditions.OfType<ConditionGroup>())
        {
            // Search in inner conditions.
            result = FindConditionGroup(nestedCondition, groupConditionId);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static ConditionGroup FindConditionParent(ConditionGroup condition, string conditionId)
    {
        ConditionGroup result;

        foreach (var nestedCondition in condition.Conditions)
        {
            if (string.Equals(nestedCondition.ConditionId, conditionId, StringComparison.OrdinalIgnoreCase))
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
