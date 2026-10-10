using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Drivers;

/// <summary>
/// A base class for the display drivers of step types. It renders the step's summary on the designer canvas (the
/// <c>{StepName}_Fields_Design</c> shape) and its editor (the <c>{StepName}_Fields_Edit</c> shape), and saves the
/// edited settings.
/// </summary>
/// <typeparam name="TSettings">The type of the step's settings.</typeparam>
/// <typeparam name="TViewModel">The type of the editor's view model.</typeparam>
public abstract class DataPipelineStepDisplayDriver<TSettings, TViewModel> : DisplayDriver<DataPipelineStep>
    where TSettings : class, new()
    where TViewModel : class, new()
{
    /// <summary>
    /// Gets the name of the step type the driver handles.
    /// </summary>
    protected abstract string StepName { get; }

    public override bool CanHandleModel(DataPipelineStep model) => model?.Type == StepName;

    public override IDisplayResult Display(DataPipelineStep step, BuildDisplayContext context)
        => Initialize<TViewModel>($"{StepName}_Fields_Design", model => EditAsync(step, DataPipelineStepType<TSettings>.GetSettings(step), model))
            .Location("Design", "Content");

    public override IDisplayResult Edit(DataPipelineStep step, BuildEditorContext context)
        => Initialize<TViewModel>($"{StepName}_Fields_Edit", model => EditAsync(step, DataPipelineStepType<TSettings>.GetSettings(step), model))
            .Location("Content");

    public override async Task<IDisplayResult> UpdateAsync(DataPipelineStep step, UpdateEditorContext context)
    {
        var model = new TViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var settings = DataPipelineStepType<TSettings>.GetSettings(step);
        await UpdateAsync(step, settings, model, context);
        DataPipelineStepType<TSettings>.SetSettings(step, settings);

        return Edit(step, context);
    }

    /// <summary>
    /// Fills the view model of the editor, and of the summary, from the settings.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="settings">The settings of the step.</param>
    /// <param name="model">The view model.</param>
    /// <returns>A task that completes when the view model is filled.</returns>
    protected abstract ValueTask EditAsync(DataPipelineStep step, TSettings settings, TViewModel model);

    /// <summary>
    /// Applies the posted view model to the settings. Report invalid values with
    /// <c>context.Updater.ModelState.AddModelError</c>.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="settings">The settings to change.</param>
    /// <param name="model">The posted view model.</param>
    /// <param name="context">The update context.</param>
    /// <returns>A task that completes when the settings are changed.</returns>
    protected abstract ValueTask UpdateAsync(DataPipelineStep step, TSettings settings, TViewModel model, UpdateEditorContext context);
}
