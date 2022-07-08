using System;
using System.Linq;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.UserTasks.Activities;
using OrchardCore.Workflows.UserTasks.ViewModels;

namespace OrchardCore.Workflows.UserTasks.Drivers
{
    public class UserTaskEventDisplayDriver : ActivityDisplayDriver<UserTaskEvent, UserTaskEventViewModel>
    {
        protected override void EditActivity(UserTaskEvent activity, UserTaskEventViewModel model)
        {
            model.Actions = string.Join(", ", activity.Actions);
            model.Roles = activity.Roles;
        }

        protected override void UpdateActivity(UserTaskEventViewModel model, UserTaskEvent activity)
        {
            activity.Actions = model.Actions.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            activity.Roles = model.Roles;
        }
    }
}
