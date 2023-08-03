using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Notifications.Activities;

public abstract class NotifyUserTaskActivity : TaskActivity
{
    protected readonly INotificationService _notificationService;
    protected readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;
    protected readonly HtmlEncoder _htmlEncoder;
    protected readonly ILogger _logger;

    public NotifyUserTaskActivity(
        INotificationService notificationService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        HtmlEncoder htmlEncoder,
        ILogger logger,
        IStringLocalizer localizer
    )
    {
        _notificationService = notificationService;
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

    public WorkflowExpression<string> TextBody
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> HtmlBody
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public bool IsHtmlPreferred
    {
        get => GetProperty(() => false);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"], S["Failed: no user found"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var users = await GetUsersAsync(workflowContext, activityContext);

        if (users == null || !users.Any())
        {
            return Outcomes("Failed: no user found");
        }

        var message = await GetMessageAsync(workflowContext);

        var totalSent = 0;

        foreach (var user in users)
        {
            totalSent += await _notificationService.SendAsync(user, message);
        }

        workflowContext.LastResult = totalSent;

        if (totalSent == 0)
        {
            return Outcomes("Failed");
        }

        return Outcomes("Done");
    }

    protected virtual async Task<INotificationMessage> GetMessageAsync(WorkflowExecutionContext workflowContext)
    {
        return new NotificationMessage()
        {
            Summary = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null),
            TextBody = await _expressionEvaluator.EvaluateAsync(TextBody, workflowContext, null),
            HtmlBody = await _expressionEvaluator.EvaluateAsync(HtmlBody, workflowContext, _htmlEncoder),
            IsHtmlPreferred = IsHtmlPreferred,
        };
    }

    abstract public override string Name { get; }

    abstract public override LocalizedString DisplayText { get; }

    abstract protected Task<IEnumerable<IUser>> GetUsersAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);
}
