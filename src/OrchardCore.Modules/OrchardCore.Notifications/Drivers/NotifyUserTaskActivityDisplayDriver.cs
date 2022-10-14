using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public class NotifyUserTaskActivityDisplayDriver<TActivity> : ActivityDisplayDriver<TActivity>
    where TActivity : NotifyUserTaskActivity
{
    private static readonly string EditShapeType = $"{typeof(NotifyUserTaskActivity).Name}_Fields_Edit";

    public override IDisplayResult Edit(TActivity model)
    {
        return Initialize(EditShapeType, (Func<NotifyUserTaskActivityViewModel, ValueTask>)(viewModel =>
        {
            return EditActivityAsync(model, viewModel);
        })).Location("Content");
    }


    public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
    {
        var viewModel = new NotifyUserTaskActivityViewModel();
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            await UpdateActivityAsync(viewModel, model);
        }

        return Edit(model);
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected virtual ValueTask EditActivityAsync(TActivity activity, NotifyUserTaskActivityViewModel model)
    {
        EditActivity(activity, model);

        return new ValueTask();
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected virtual void EditActivity(TActivity activity, NotifyUserTaskActivityViewModel model)
    {
        model.Subject = activity.Subject.Expression;
        model.Body = activity.Body.Expression;
        model.IsHtmlBody = activity.IsHtmlBody;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected virtual Task UpdateActivityAsync(NotifyUserTaskActivityViewModel model, TActivity activity)
    {
        UpdateActivity(model, activity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected virtual void UpdateActivity(NotifyUserTaskActivityViewModel model, TActivity activity)
    {
        activity.Subject = new WorkflowExpression<string>(model.Subject);
        activity.Body = new WorkflowExpression<string>(model.Body);
        activity.IsHtmlBody = model.IsHtmlBody;
    }

    public override IDisplayResult Display(TActivity activity)
    {
        return Combine(
            Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape($"{typeof(TActivity).Name}_Fields_Design", new ActivityViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }
}
