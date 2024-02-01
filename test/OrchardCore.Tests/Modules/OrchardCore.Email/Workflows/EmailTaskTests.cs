using OrchardCore.Email;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Events;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Modules.Email.Workflows.Tests
{
    public class EmailTaskTests
    {
        private static readonly IDictionary<string, object> _emptyDictionary = new Dictionary<string, object>();

        [Fact]
        public async Task ExecuteTask_WhenToAndCcAndBccAreNotSet_ShouldFails()
        {
            // Arrange
            var emailSettingsOptions = Options.Create(new SmtpEmailSettings());
            var emailMessageValidator = new EmailMessageValidator(
                new EmailAddressValidator(),
                emailSettingsOptions,
                Mock.Of<IStringLocalizer<EmailMessageValidator>>());
            var emailDeliveryServiceResolver = new EmailDeliveryServiceResolver(Mock.Of<IServiceProvider>());
            var emailService = new EmailService(
                emailMessageValidator,
                emailDeliveryServiceResolver,
                Enumerable.Empty<IEmailServiceEvents>(),
                NullLogger<EmailService>.Instance);
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

        private class SimpleWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
        {
            public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
                => await Task.FromResult((T)Convert.ChangeType(expression.Expression, typeof(T)));
        }
    }
}
