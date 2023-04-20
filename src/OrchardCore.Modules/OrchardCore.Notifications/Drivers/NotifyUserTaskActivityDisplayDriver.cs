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

public class NotifyUserTaskActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity, TEditViewModel>
    where TActivity : NotifyUserTaskActivity
    where TEditViewModel : NotifyUserTaskActivityViewModel, new()
{
    protected virtual string EditShapeType => $"{typeof(NotifyUserTaskActivity).Name}_Fields_Edit";

    public override IDisplayResult Edit(TActivity model)
    {
        return Initialize(EditShapeType, (Func<TEditViewModel, ValueTask>)(viewModel =>
        {
            return EditActivityAsync(model, viewModel);
        })).Location("Content");
    }

    public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
    {
        var viewModel = new TEditViewModel();
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            await UpdateActivityAsync(viewModel, model);
        }

        return Edit(model);
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override ValueTask EditActivityAsync(TActivity activity, TEditViewModel model)
    {
        EditActivity(activity, model);

        return new ValueTask();
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override void EditActivity(TActivity activity, TEditViewModel model)
    {
        model.Subject = activity.Subject.Expression;
        model.TextBody = activity.TextBody.Expression;
        model.HtmlBody = activity.HtmlBody.Expression;
        model.IsHtmlPreferred = activity.IsHtmlPreferred;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override Task UpdateActivityAsync(TEditViewModel model, TActivity activity)
    {
        UpdateActivity(model, activity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override void UpdateActivity(TEditViewModel model, TActivity activity)
    {
        activity.Subject = new WorkflowExpression<string>(model.Subject);
        activity.TextBody = new WorkflowExpression<string>(model.TextBody);
        activity.HtmlBody = new WorkflowExpression<string>(model.HtmlBody);
        activity.IsHtmlPreferred = model.IsHtmlPreferred;
    }

    public override IDisplayResult Display(TActivity activity)
    {
        return Combine(
            Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape($"{typeof(TActivity).Name}_Fields_Design", new ActivityViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }
}

public class NotifyUserTaskActivityDisplayDriver<TActivity> : NotifyUserTaskActivityDisplayDriver<TActivity, NotifyUserTaskActivityViewModel>
        where TActivity : NotifyUserTaskActivity
{
}
