using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
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
       INotificationService notificationCoordinator,
       IWorkflowExpressionEvaluator expressionEvaluator,
       HtmlEncoder htmlEncoder,
       ILogger<NotifyContentOwnerTask> logger,
       IStringLocalizer<NotifyContentOwnerTask> localizer,
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

    protected override async Task<IEnumerable<IUser>> GetUsersAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Input.TryGetValue("ContentItem", out var obj)
            && obj is ContentItem contentItem
            && !String.IsNullOrEmpty(contentItem.Owner))
        {
            if (workflowContext.Input.TryGetValue("Owner", out var ownerObject) && ownerObject is User user && user.IsEnabled)
            {
                return new[] { user };
            }

            var owner = await _session.Query<User, UserIndex>(x => x.UserId == contentItem.Owner && x.IsEnabled).FirstOrDefaultAsync();

            if (owner != null)
            {
                workflowContext.Input.TryAdd("Owner", owner);

                return new[] { owner };
            }
        }

        return Enumerable.Empty<IUser>();
    }
}
