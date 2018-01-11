using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Email.Activities
{
    // TODO: Move this to the OrchardCore.Email module when available.
    // This implementation should not be considered complete, but a starting point.
    public class EmailTask : TaskActivity
    {
        private readonly IOptions<SmtpOptions> _smtpOptions;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailTask> _logger;

        public EmailTask(IStringLocalizer<EmailTask> localizer, IOptions<SmtpOptions> smtpOptions, ILiquidTemplateManager liquidTemplateManager, IServiceProvider serviceProvider, ILogger<EmailTask> logger)
        {
            _smtpOptions = smtpOptions;
            _liquidTemplateManager = liquidTemplateManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(EmailTask);
        public override LocalizedString Category => T["Messaging"];
        public override LocalizedString Description => T["Send an email."];

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

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var host = _smtpOptions.Value.Host;
            var port = _smtpOptions.Value.Port;

            using (var smtpClient = new SmtpClient(host, port))
            {
                var sender = (await workflowContext.EvaluateExpressionAsync(Sender))?.Trim();
                var recipients = await workflowContext.EvaluateExpressionAsync(Recipients);
                var subject = (await workflowContext.EvaluateExpressionAsync(Subject))?.Trim();
                var body = await workflowContext.EvaluateExpressionAsync(Body);
                var mailMessage = new MailMessage(sender, recipients, subject, body) { IsBodyHtml = true };

                await smtpClient.SendMailAsync(mailMessage);

                return new[] { "Done" };
            }
        }
    }
}