using OrchardCore.Email;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Email.Workflows;

public class EmailTaskTests
{
    private static readonly IDictionary<string, object> _emptyDictionary = new Dictionary<string, object>();

    [Fact]
    public async Task ExecuteTask_WhenToAndCcAndBccAreNotSet_ShouldFail()
    {
        // Arrange
        var emailService = CreateSmtpService(new SmtpOptions()
        {
            IsEnabled = true,
        });

        var task = new EmailTask(
            emailService,
            new SimpleWorkflowExpressionEvaluator(),
            Mock.Of<IStringLocalizer<EmailTask>>(),
            HtmlEncoder.Default)
        {
            Subject = new WorkflowExpression<string>("Test"),
            TextBody = new WorkflowExpression<string>("Test message!!"),
        };

        var executionContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow(),
            _emptyDictionary,
            _emptyDictionary,
            _emptyDictionary,
            [],
            default,
            []);
        var activityContext = Mock.Of<ActivityContext>();

        // Act
        var result = await task.ExecuteAsync(executionContext, activityContext);

        // Assert
        Assert.Equal("Failed", result.Outcomes.First());
    }

    private static EmailService CreateSmtpService(SmtpOptions smtpOptions)
    {
        var options = new Mock<IOptions<SmtpOptions>>();
        var logger = new Mock<ILogger<SmtpEmailProvider>>();
        var logger2 = new Mock<ILogger<EmailService>>();

        var localizer = new Mock<IStringLocalizer<SmtpEmailProvider>>();
        var emailServiceLocalizer = new Mock<IStringLocalizer<EmailService>>();
        var emailValidator = new Mock<IEmailAddressValidator>();

        emailValidator.Setup(x => x.Validate(It.IsAny<string>()))
            .Returns(true);

        options.Setup(o => o.Value)
            .Returns(smtpOptions);

        var smtp = new SmtpEmailProvider(options.Object, emailValidator.Object, logger.Object, localizer.Object);

        var emailProviderFactory = new Mock<IEmailProviderFactory>();
        emailProviderFactory.Setup(f => f.GetProvider(It.IsAny<string>()))
            .Returns(smtp);

        var emailService = new Mock<IEmailService>();

        return new EmailService(emailProviderFactory.Object, Options.Create(new EmailOptions()));
    }

    private sealed class SimpleWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
    {
        public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
            => await Task.FromResult((T)Convert.ChangeType(expression.Expression, typeof(T)));
    }
}
