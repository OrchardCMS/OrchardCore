using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Display;

/// <summary>
/// Base class for ETL activity display drivers.
/// </summary>
public abstract class EtlActivityDisplayDriver<TActivity> : DisplayDriver<IEtlActivity, TActivity>
    where TActivity : class, IEtlActivity
{
    protected static readonly string ActivityName = typeof(TActivity).Name;

    private static readonly string _thumbnailShapeType = $"{ActivityName}_Fields_Thumbnail";
    private static readonly string _designShapeType = $"{ActivityName}_Fields_Design";

    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape(_thumbnailShapeType, new EtlActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape(_designShapeType, new EtlActivityViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }
}

/// <summary>
/// Base class for ETL activity display drivers using a strongly typed view model.
/// </summary>
public abstract class EtlActivityDisplayDriver<TActivity, TEditViewModel> : EtlActivityDisplayDriver<TActivity>
    where TActivity : class, IEtlActivity
    where TEditViewModel : class, new()
{
    private static readonly string _editShapeType = $"{ActivityName}_Fields_Edit";

    public override IDisplayResult Edit(TActivity activity, BuildEditorContext context)
    {
        return Initialize<TEditViewModel>(_editShapeType, viewModel => EditActivityAsync(activity, viewModel))
            .Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(TActivity activity, UpdateEditorContext context)
    {
        var viewModel = new TEditViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        await UpdateActivityAsync(viewModel, activity);

        return Edit(activity, context);
    }

    /// <summary>
    /// Populates the view model before it is rendered in the editor.
    /// </summary>
    protected virtual ValueTask EditActivityAsync(TActivity activity, TEditViewModel model)
    {
        EditActivity(activity, model);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Populates the view model before it is rendered in the editor.
    /// </summary>
    protected virtual void EditActivity(TActivity activity, TEditViewModel model)
    {
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected virtual Task UpdateActivityAsync(TEditViewModel model, TActivity activity)
    {
        UpdateActivity(model, activity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected virtual void UpdateActivity(TEditViewModel model, TActivity activity)
    {
    }
}
