using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Email.Activities;
using OrchardCore.Workflows.Email.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Email.Drivers
{
    public class EmailTaskDisplay : ActivityDisplayDriver<EmailTask, EmailTaskViewModel>
    {
        protected override void Map(EmailTask source, EmailTaskViewModel target)
        {
            target.SenderExpression = source.Sender.Expression;
            target.RecipientsExpression = source.Recipients.Expression;
            target.SubjectExpression = source.Subject.Expression;
            target.Body = source.Body.Expression;
        }

        protected override void Map(EmailTaskViewModel source, EmailTask target)
        {
            target.Sender = new WorkflowExpression<string>(source.SenderExpression);
            target.Recipients = new WorkflowExpression<string>(source.RecipientsExpression);
            target.Subject = new WorkflowExpression<string>(source.SubjectExpression);
            target.Body = new WorkflowExpression<string>(source.Body);
        }
    }
}
