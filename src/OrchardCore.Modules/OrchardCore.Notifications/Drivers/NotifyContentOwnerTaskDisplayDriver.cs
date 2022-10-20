using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Notifications.Drivers;

public class NotifyContentOwnerTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyContentOwnerTask, NotifyContentOwnerActivityViewModel>
{
    public override IDisplayResult Edit(NotifyContentOwnerTask model)
    {
        return Combine(
            base.Edit(model),
            Initialize($"{typeof(NotifyContentOwnerTask).Name}_Fields_Edit", (Func<NotifyContentOwnerActivityViewModel, ValueTask>)(viewModel =>
            {
                return EditActivityAsync(model, viewModel);
            })).Location("Content")
        );
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override void EditActivity(NotifyContentOwnerTask activity, NotifyContentOwnerActivityViewModel model)
    {
        base.EditActivity(activity, model);

        model.LinkType = activity.LinkType;
        model.Body = activity.Body.Expression;
        model.IsHtmlBody = activity.IsHtmlBody;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override void UpdateActivity(NotifyContentOwnerActivityViewModel model, NotifyContentOwnerTask activity)
    {
        base.UpdateActivity(model, activity);

        activity.LinkType = model.LinkType;
        activity.Url = new WorkflowExpression<string>(model.Url);
    }
}
