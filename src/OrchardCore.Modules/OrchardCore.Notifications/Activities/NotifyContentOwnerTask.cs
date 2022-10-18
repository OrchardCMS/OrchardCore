using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Notifications.Activities;

public class NotifyContentOwnerTask : NotifyUserTaskActivity
{
    private readonly ISession _session;

    public NotifyContentOwnerTask(
       INotificationManager notificationCoordinator,
       IWorkflowExpressionEvaluator expressionEvaluator,
       HtmlEncoder htmlEncoder,
       ILogger<NotifyUserTask> logger,
       IStringLocalizer<NotifyUserTask> localizer,
       ISession session
   ) : base(notificationCoordinator,
       expressionEvaluator,
       htmlEncoder,
       logger,
       localizer)
    {
        _session = session;
    }

    public override string Name => nameof(NotifyContentOwnerTask);

    public override LocalizedString DisplayText => S["Notify Content's Owner Task"];

    public WorkflowExpression<string> LinkType
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Url
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    protected override async Task<IUser> GetUserAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Input.TryGetValue("ContentItem", out var obj)
            && obj is ContentItem contentItem
            && !String.IsNullOrEmpty(contentItem.Owner))
        {
            var owner = await _session.Query<User, UserIndex>(x => x.UserId == contentItem.Owner).FirstOrDefaultAsync();

            workflowContext.Input.TryAdd("Owner", owner);

            return owner;
        }

        return null;
    }

    protected override async Task<INotificationMessage> GetMessageAsync(WorkflowExecutionContext workflowContext)
    {
        var message = new ContentNotificationMessage()
        {
            Subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, _htmlEncoder),
        };

        if (LinkType.Expression == "content" && workflowContext.Input.TryGetValue("ContentItem", out var obj) && obj is ContentItem contentItem)
        {
            message.ContentItemId = contentItem.ContentItemId;
        }
        else if (LinkType.Expression == "url" && !String.IsNullOrWhiteSpace(Url.Expression))
        {
            message.Url = Url.Expression;
        }
        else
        {
            message.Body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, _htmlEncoder);
            message.BodyContainsHtml = IsHtmlBody;
        }

        return message;
    }
}
