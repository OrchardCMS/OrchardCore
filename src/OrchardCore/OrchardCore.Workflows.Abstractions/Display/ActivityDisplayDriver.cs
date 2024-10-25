using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Display;

/// <summary>
/// Base class for activity drivers.
/// </summary>
public abstract class ActivityDisplayDriver<TActivity> : DisplayDriver<IActivity, TActivity>
    where TActivity : class, IActivity
{
    protected static readonly string ActivityName = typeof(TActivity).Name;

    private static readonly string _thumbnailShapeType = $"{ActivityName}_Fields_Thumbnail";
    private static readonly string _designShapeType = $"{ActivityName}_Fields_Design";

    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape(_thumbnailShapeType, new ActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape(_designShapeType, new ActivityViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }
}

/// <summary>
/// Base class for activity drivers using a strongly typed view model.
/// </summary>
public abstract class ActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity>
    where TActivity : class, IActivity where TEditViewModel : class, new()
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
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected virtual ValueTask EditActivityAsync(TActivity activity, TEditViewModel model)
    {
        EditActivity(activity, model);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
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
