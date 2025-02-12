using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Email.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Email.Workflows.Drivers;

public sealed class EmailTaskDisplayDriver : ActivityDisplayDriver<EmailTask, EmailTaskViewModel>
{
    protected override void EditActivity(EmailTask activity, EmailTaskViewModel model)
    {
        model.SenderExpression = activity.Sender.Expression;
        model.AuthorExpression = activity.Author.Expression;
        model.RecipientsExpression = activity.Recipients.Expression;
        model.ReplyToExpression = activity.ReplyTo.Expression;
        model.SubjectExpression = activity.Subject.Expression;
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
        activity.TextBody = new WorkflowExpression<string>(model.TextBody);
        activity.HtmlBody = new WorkflowExpression<string>(model.HtmlBody);
        activity.Bcc = new WorkflowExpression<string>(model.BccExpression);
        activity.Cc = new WorkflowExpression<string>(model.CcExpression);
    }
}
