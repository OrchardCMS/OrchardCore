using OrchardCore.Email;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Services;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Email.Workflows
{
    public class EmailTaskTests
    {
        private static readonly IDictionary<string, object> _emptyDictionary = new Dictionary<string, object>();

        [Fact]
        public async Task ExecuteTask_WhenToAndCcAndBccAreNotSet_ShouldFails()
        {
            // Arrange
            var emailService = CreateSmtpService(new SmtpSettings());

            var task = new EmailTask(
                emailService,
                new SimpleWorkflowExpressionEvaluator(),
                Mock.Of<IStringLocalizer<EmailTask>>(),
                HtmlEncoder.Default)
            {
                Subject = new WorkflowExpression<string>("Test"),
                Body = new WorkflowExpression<string>("Test message!!")
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

        private static DefaultEmailService CreateSmtpService(SmtpSettings settings)
        {
            var options = new Mock<IOptions<SmtpSettings>>();
            var logger = new Mock<ILogger<SmtpEmailProvider>>();
            var localizer = new Mock<IStringLocalizer<SmtpEmailProvider>>();
            var emailServiceLocalizer = new Mock<IStringLocalizer<DefaultEmailService>>();
            var smtp = new SmtpEmailProvider(options.Object, logger.Object, localizer.Object);

            var resolver = new Mock<IEmailProviderResolver>();
            resolver.Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<IEmailProvider>(smtp));

            var emailService = new Mock<IEmailService>();

            options.Setup(o => o.Value)
                .Returns(settings);

            return new DefaultEmailService(resolver.Object, emailServiceLocalizer.Object);
        }

        private class SimpleWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
        {
            public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
                => await Task.FromResult((T)Convert.ChangeType(expression.Expression, typeof(T)));
        }
    }
}
