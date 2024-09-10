using System.Text.Encodings.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Notifications.Activities;

public class NotifyUserTask : NotifyUserTaskActivity<NotifyUserTask>
{
    private readonly ISession _session;

    public NotifyUserTask(
       INotificationService notificationCoordinator,
       IWorkflowExpressionEvaluator expressionEvaluator,
       HtmlEncoder htmlEncoder,
       ILogger<NotifyUserTask> logger,
       IStringLocalizer<NotifyUserTask> stringLocalizer,
       ISession session
   ) : base(notificationCoordinator,
       expressionEvaluator,
       htmlEncoder,
       logger,
       stringLocalizer)
    {
        _session = session;
    }

    public override LocalizedString DisplayText => S["Notify Specific Users Task"];

    public WorkflowExpression<string> UserNames
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    protected override async Task<IEnumerable<IUser>> GetUsersAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!string.IsNullOrEmpty(UserNames.Expression))
        {
            var expression = await _expressionEvaluator.EvaluateAsync(UserNames, workflowContext, null);

            var userNames = expression.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (userNames.Length > 0)
            {
                var users = new List<User>();

                foreach (var page in userNames.PagesOf(1000))
                {
                    users.AddRange(await _session.Query<User, UserIndex>(user => user.NormalizedUserName.IsIn(page)).ListAsync());
                }

                return users;
            }
        }

        return [];
    }
}
