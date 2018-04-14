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
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Email.Activities
{
    // TODO: Move this to the OrchardCore.Email module when available.
    // This implementation should not be considered complete, but a starting point.
    public class EmailTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IOptions<SmtpOptions> _smtpOptions;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailTask> _logger;

        public EmailTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EmailTask> localizer, 
            IOptions<SmtpOptions> smtpOptions, 
            ILiquidTemplateManager liquidTemplateManager, 
            IServiceProvider serviceProvider, 
            ILogger<EmailTask> logger
        )
        {
            _expressionEvaluator = expressionEvaluator;
            _smtpOptions = smtpOptions;
            _liquidTemplateManager = liquidTemplateManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
            T = localizer;
        }
        
        private IStringLocalizer T { get; }
        public override string Name => nameof(EmailTask);
        public override LocalizedString Category => T["Messaging"];

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

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var host = _smtpOptions.Value.Host;
            var port = _smtpOptions.Value.Port;

            using (var smtpClient = new SmtpClient(host, port))
            {
                var senderTask = _expressionEvaluator.EvaluateAsync(Sender, workflowContext);
                var recipientsTask = _expressionEvaluator.EvaluateAsync(Recipients, workflowContext);
                var subjectTask = _expressionEvaluator.EvaluateAsync(Subject, workflowContext);
                var bodyTask = _expressionEvaluator.EvaluateAsync(Body, workflowContext);

                await Task.WhenAll(senderTask, recipientsTask, subjectTask, bodyTask);
                var mailMessage = new MailMessage(senderTask.Result?.Trim(), recipientsTask.Result, subjectTask.Result?.Trim(), bodyTask.Result?.Trim())
                {
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);

                return Outcomes("Done");
            }
        }
    }
}