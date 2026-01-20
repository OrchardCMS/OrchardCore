using System.Text.Encodings.Web;
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

    public WorkflowExpression<string> Summary
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
        => Outcomes(S["Done"], S["Failed"], S["Failed: no user found"]);

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
            Subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null),
            Summary = await _expressionEvaluator.EvaluateAsync(Summary, workflowContext, _htmlEncoder),
            TextBody = await _expressionEvaluator.EvaluateAsync(TextBody, workflowContext, null),
            HtmlBody = await _expressionEvaluator.EvaluateAsync(HtmlBody, workflowContext, _htmlEncoder),
            IsHtmlPreferred = IsHtmlPreferred,
        };
    }

    public abstract override string Name { get; }

    public abstract override LocalizedString DisplayText { get; }

    protected abstract Task<IEnumerable<IUser>> GetUsersAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);
}

public abstract class NotifyUserTaskActivity<TActivity> : NotifyUserTaskActivity where TActivity : ITask
{
    protected NotifyUserTaskActivity(
        INotificationService notificationService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        HtmlEncoder htmlEncoder,
        ILogger logger,
        IStringLocalizer localizer)
        : base(notificationService, expressionEvaluator, htmlEncoder, logger, localizer)
    {
    }

    // The technical name of the activity. Within a workflow definition, activities make use of this name.
    public override string Name => typeof(TActivity).Name;
}
