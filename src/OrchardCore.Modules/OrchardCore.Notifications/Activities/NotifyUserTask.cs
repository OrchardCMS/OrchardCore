using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Notifications.Activities;

public class NotifyUserTask : NotifyUserTaskActivity
{
    public NotifyUserTask(
       INotificationService notificationCoordinator,
       IWorkflowExpressionEvaluator expressionEvaluator,
       HtmlEncoder htmlEncoder,
       ILogger<NotifyUserTask> logger,
       IStringLocalizer<NotifyUserTask> localizer
   ) : base(notificationCoordinator,
       expressionEvaluator,
       htmlEncoder,
       logger,
       localizer)
    {
    }

    public override string Name => nameof(NotifyUserTask);

    public override LocalizedString DisplayText => S["Notify User Task"];

    protected override Task<IEnumerable<IUser>> GetUsersAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Input.TryGetValue("User", out var userObject) && userObject is User user && user.IsEnabled)
        {
            return Task.FromResult<IEnumerable<IUser>>(new[] { user });
        }

        return Task.FromResult(Enumerable.Empty<IUser>());
    }
}
