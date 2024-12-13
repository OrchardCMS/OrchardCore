using System.Text.Encodings.Web;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Email.Workflows.Activities;

public class EmailTask : TaskActivity<EmailTask>
{
    private readonly IEmailService _emailService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;
    private readonly HtmlEncoder _htmlEncoder;

    public EmailTask(
        IEmailService emailService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<EmailTask> localizer,
        HtmlEncoder htmlEncoder
    )
    {
        _emailService = emailService;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
        _htmlEncoder = htmlEncoder;
    }

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

    public WorkflowExpression<string> ReplyTo
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

    public WorkflowExpression<string> Cc
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Bcc
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

    public bool IsHtmlBody
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
        var replyTo = await _expressionEvaluator.EvaluateAsync(ReplyTo, workflowContext, null);
        var recipients = await _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
        var cc = await _expressionEvaluator.EvaluateAsync(Cc, workflowContext, null);
        var bcc = await _expressionEvaluator.EvaluateAsync(Bcc, workflowContext, null);
        var subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null);
        var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, IsHtmlBody ? _htmlEncoder : null);

        var message = new MailMessage
        {
            // Author and Sender are both not required fields.
            From = author?.Trim() ?? sender?.Trim(),
            To = recipients?.Trim(),
            Cc = cc?.Trim(),
            Bcc = bcc?.Trim(),
            // Email reply-to header https://tools.ietf.org/html/rfc4021#section-2.1.4
            ReplyTo = replyTo?.Trim(),
            Subject = subject?.Trim(),
            Body = body?.Trim(),
            IsHtmlBody = IsHtmlBody
        };

        if (!string.IsNullOrWhiteSpace(sender))
        {
            message.Sender = sender.Trim();
        }

        var result = await _emailService.SendAsync(message);
        workflowContext.LastResult = result;

        if (!result.Succeeded)
        {
            return Outcomes("Failed");
        }

        return Outcomes("Done");
    }
}
