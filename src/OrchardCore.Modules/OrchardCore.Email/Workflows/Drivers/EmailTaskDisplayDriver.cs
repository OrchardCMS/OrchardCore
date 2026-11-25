using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Email.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Email.Workflows.Drivers;

public sealed class EmailTaskDisplayDriver : ActivityDisplayDriver<EmailTask, EmailTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public EmailTaskDisplayDriver(IStringLocalizer<EmailTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EmailTask activity, BuildEditorContext context)
    {
        return Combine(
            Initialize<EmailTaskViewModel>("EmailTask_Fields_Edit", model => EditActivityAsync(activity, model))
                .Location(l => l.Zone("Content", "1").Card(S["Addresses"], "10")),
            Initialize<EmailTaskViewModel>("EmailTaskMessage_Fields_Edit", model => EditActivityAsync(activity, model))
                .Location(l => l.Zone("Content", "1").Card(S["Message"], "20")));
    }

    protected override void EditActivity(EmailTask activity, EmailTaskViewModel model)
    {
        model.SenderExpression = activity.Sender.Expression;
        model.AuthorExpression = activity.Author.Expression;
        model.RecipientsExpression = activity.Recipients.Expression;
        model.ReplyToExpression = activity.ReplyTo.Expression;
        model.SubjectExpression = activity.Subject.Expression;
        model.BodyFormat = activity.BodyFormat;
        model.TextBody = activity.TextBody.Expression;
        model.HtmlBody = activity.HtmlBody.Expression;
        model.BccExpression = activity.Bcc.Expression;
        model.CcExpression = activity.Cc.Expression;
    }

    protected override void UpdateActivity(EmailTaskViewModel model, EmailTask activity)
    {
        activity.Sender = new WorkflowExpression<string>(model.SenderExpression);
        activity.Author = new WorkflowExpression<string>(model.AuthorExpression);
        activity.Recipients = new WorkflowExpression<string>(model.RecipientsExpression);
        activity.ReplyTo = new WorkflowExpression<string>(model.ReplyToExpression);
        activity.Subject = new WorkflowExpression<string>(model.SubjectExpression);
        activity.BodyFormat = model.BodyFormat;
        activity.TextBody = new WorkflowExpression<string>(model.TextBody);
        activity.HtmlBody = new WorkflowExpression<string>(model.HtmlBody);
        activity.Bcc = new WorkflowExpression<string>(model.BccExpression);
        activity.Cc = new WorkflowExpression<string>(model.CcExpression);
    }
}
