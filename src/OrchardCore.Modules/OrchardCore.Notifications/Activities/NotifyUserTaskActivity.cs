using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Notifications.Activities;

public abstract class NotifyUserTaskActivity : TaskActivity
{
    protected readonly INotificationManager _notificationCoordinator;
    protected readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;
    protected readonly HtmlEncoder _htmlEncoder;
    protected readonly ILogger _logger;

    public NotifyUserTaskActivity(
        INotificationManager notificationCoordinator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        HtmlEncoder htmlEncoder,
        ILogger<NotifyUserTask> logger,
        IStringLocalizer<NotifyUserTask> localizer
    )
    {
        _notificationCoordinator = notificationCoordinator;
        _expressionEvaluator = expressionEvaluator;
        _htmlEncoder = htmlEncoder;
        _logger = logger;

        S = localizer;
    }

    public override LocalizedString Category => S["Notifications"];

    public WorkflowExpression<string> Subject
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Body
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public bool IsHtmlBody
    {
        get => GetProperty(() => false);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"], S["Failed: user not found"], S["Failed: uisabled User"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var user = await GetUserAsync(workflowContext, activityContext);

        if (user == null)
        {
            return Outcomes("Failed: user not found");
        }

        if (user is User su && !su.IsEnabled)
        {
            return Outcomes("Failed: disabled User");
        }

        var message = new HtmlNotificationMessage()
        {
            Subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, _htmlEncoder),
            Body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, _htmlEncoder),
            BodyContainsHtml = IsHtmlBody
        };

        var result = await _notificationCoordinator.SendAsync(user, message);
        workflowContext.LastResult = result;

        if (result == 0)
        {
            return Outcomes("Failed");
        }

        return Outcomes("Done");
    }

    abstract public override string Name { get; }

    abstract public override LocalizedString DisplayText { get; }

    abstract protected Task<IUser> GetUserAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);
}
