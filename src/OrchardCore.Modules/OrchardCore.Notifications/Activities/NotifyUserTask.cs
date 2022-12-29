using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Users;
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

    protected override Task<IUser> GetUserAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Input.TryGetValue("User", out var userObject) && userObject is IUser user)
        {
            return Task.FromResult(user);
        }

        return Task.FromResult<IUser>(null);
    }
}
