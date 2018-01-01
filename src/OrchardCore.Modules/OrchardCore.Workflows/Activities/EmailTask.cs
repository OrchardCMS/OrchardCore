using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    // TODO: Move this to the OrchardCore.Email module when available.
    // This implementation should not be considered complete, but a starting point.
    public class EmailTask : TaskActivity
    {

        public EmailTask(IStringLocalizer<EmailTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(EmailTask);
        public override LocalizedString Category => T["Networking"];
        public override LocalizedString Description => T["Send an email."];

        public WorkflowExpression<string> SenderExpression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // TODO: Add support for the following format: Jack Bauer<jack@ctu.com>, ...
        public WorkflowExpression<IList<string>> RecipientsExpression
        {
            get => GetProperty(() => new WorkflowExpression<IList<string>>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> SubjectExpression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // TODO: Integrate with templating support (Liquid).
        public string Body
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            // TODO: Read host and credentials from configuration.
            using (var smtpClient = new SmtpClient("localhost"))
            {
                var sender = workflowContext.Evaluate(SenderExpression);
                var recipients = string.Join(";", (workflowContext.Evaluate(RecipientsExpression) ?? new List<string>()));
                var subject = workflowContext.Evaluate(SubjectExpression)?.Trim();
                var mailMessage = new MailMessage(sender, recipients, subject, Body);

                await smtpClient.SendMailAsync(mailMessage);

                return new[] { "Done" };
            }
        }
    }
}