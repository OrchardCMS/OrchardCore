using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;
using OrchardCore.Modules;
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

        public WorkflowExpression<string> SenderExpression
        {
            get => GetProperty(() => new StringWorkflowExpression());
            set => SetProperty(value);
        }

        // TODO: Add support for the following format: Jack Bauer<jack@ctu.com>, ...
        public WorkflowExpression<IList<string>> RecipientsExpression
        {
            get => GetProperty(() => new StringListWorkflowExpression());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> SubjectExpression
        {
            get => GetProperty(() => new StringWorkflowExpression());
            set => SetProperty(value);
        }

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
            var host = _smtpOptions.Value.Host;
            var port = _smtpOptions.Value.Port;

            using (var smtpClient = new SmtpClient(host, port))
            {
                var sender = await workflowContext.EvaluateAsync(SenderExpression);
                var recipients = string.Join(";", (await workflowContext.EvaluateAsync(RecipientsExpression) ?? new List<string>()));
                var subject = (await workflowContext.EvaluateAsync(SubjectExpression))?.Trim();
                var body = await RenderLiquidAsync(Body, workflowContext, activityContext);
                var mailMessage = new MailMessage(sender, recipients, subject, body) { IsBodyHtml = true };

                await smtpClient.SendMailAsync(mailMessage);

                return new[] { "Done" };
            }
        }

        /// <summary>
        /// Renders the specified liquid string into a HTML string.
        /// </summary>
        private async Task<string> RenderLiquidAsync(string liquid, WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var templateContext = new TemplateContext();
            templateContext.SetValue("WorkflowContext", workflowContext);
            templateContext.SetValue("ActivityContext", activityContext);

            foreach (var item in workflowContext.Input)
            {
                templateContext.SetValue(item.Key, item.Value);
            }

            foreach (var item in workflowContext.Properties)
            {
                templateContext.SetValue(item.Key, item.Value);
            }

            templateContext.MemberAccessStrategy.Register<WorkflowContext>();
            await ContextualizeAsync(templateContext);

            using (var writer = new StringWriter())
            {
                await _liquidTemplateManager.RenderAsync(liquid, writer, HtmlEncoder.Default, templateContext);
                return writer.ToString();
            }
        }

        public async Task ContextualizeAsync(TemplateContext context)
        {
            var services = _serviceProvider;
            context.AmbientValues.Add("Services", services);

            var displayHelperFactory = services.GetRequiredService<IDisplayHelperFactory>();
            context.AmbientValues.Add("DisplayHelperFactory", displayHelperFactory);

            var actionContext = services.GetService<IActionContextAccessor>()?.ActionContext;
            if (actionContext != null)
            {
                var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
                context.AmbientValues.Add("UrlHelper", urlHelper);
            }

            var shapeFactory = services.GetRequiredService<IShapeFactory>();
            context.AmbientValues.Add("ShapeFactory", shapeFactory);

            var handlers = services.GetServices<ILiquidTemplateEventHandler>();
            await handlers.InvokeAsync(async x => await x.RenderingAsync(context), _logger);
        }
    }
}