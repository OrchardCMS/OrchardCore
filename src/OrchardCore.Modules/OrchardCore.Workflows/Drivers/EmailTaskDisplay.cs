using System.Collections.Generic;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class EmailTaskDisplay : ActivityDisplayDriver<EmailTask, EmailTaskViewModel>
    {
        protected override void Map(EmailTask source, EmailTaskViewModel target)
        {
            target.SenderExpression = source.SenderExpression.Expression;
            target.RecipientsExpression = source.RecipientsExpression.Expression;
            target.SubjectExpression = source.SubjectExpression.Expression;
            target.Body = source.Body;
        }

        protected override void Map(EmailTaskViewModel source, EmailTask target)
        {
            target.SenderExpression = new WorkflowExpression<string>(source.SenderExpression);
            target.RecipientsExpression = new WorkflowExpression<IList<string>>(source.RecipientsExpression);
            target.SubjectExpression = new WorkflowExpression<string>(source.SubjectExpression);
            target.Body = source.Body?.Trim();
        }
    }
}
