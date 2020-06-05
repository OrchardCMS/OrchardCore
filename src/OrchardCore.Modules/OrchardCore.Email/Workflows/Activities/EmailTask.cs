using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Email.Workflows.Activities
{
    public class EmailTask : TaskActivity
    {
        private readonly ISmtpService _smtpService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly HtmlEncoder _htmlEncoder;

        public EmailTask(
            ISmtpService smtpService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EmailTask> localizer,
            HtmlEncoder htmlEncoder
        )
        {
            _smtpService = smtpService;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
            _htmlEncoder = htmlEncoder;
        }

        public override string Name => nameof(EmailTask);
        public override LocalizedString DisplayText => S["Email Task"];
        public override LocalizedString Category => S["Messaging"];

        public WorkflowExpression<string> Author
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Sender
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // TODO: Add support for the following format: Jack Bauer<jack@ctu.com>, ...
        public WorkflowExpression<string> Recipients
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

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

        public bool IsBodyHtml
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var author = await _expressionEvaluator.EvaluateAsync(Author, workflowContext, null);
            var sender = await _expressionEvaluator.EvaluateAsync(Sender, workflowContext, null);
            var recipients = await _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
            var subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null);
            // Don't html-encode liquid tags if the email is not html
            var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, IsBodyHtml ? _htmlEncoder : null);

            var message = new MailMessage
            {
                // Author and Sender are both not required fields.
                From = author?.Trim() ?? sender?.Trim(),
                To = recipients.Trim(),
                Subject = subject.Trim(),
                Body = body?.Trim(),
                IsBodyHtml = IsBodyHtml
            };

            if (!String.IsNullOrWhiteSpace(sender))
            {
                message.Sender = sender.Trim();
            }

            var result = await _smtpService.SendAsync(message);
            workflowContext.LastResult = result;

            if (!result.Succeeded)
            {
                return Outcomes("Failed");
            }

            return Outcomes("Done");
        }
    }
}
