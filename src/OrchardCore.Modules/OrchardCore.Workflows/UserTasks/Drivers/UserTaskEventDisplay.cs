using System;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.UserTasks.Activities;
using OrchardCore.Workflows.UserTasks.ViewModels;

namespace OrchardCore.Workflows.UserTasks.Drivers
{
    public class UserTaskEventDisplay : ActivityDisplayDriver<UserTaskEvent, UserTaskEventViewModel>
    {
        protected override void EditActivity(UserTaskEvent activity, UserTaskEventViewModel model)
        {
            model.Actions = string.Join(", ", activity.Actions);
        }

        protected override void UpdateActivity(UserTaskEventViewModel model, UserTaskEvent activity)
        {
            activity.Actions = model.Actions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
