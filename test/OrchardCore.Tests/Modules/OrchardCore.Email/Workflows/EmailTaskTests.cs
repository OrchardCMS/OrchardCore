using OrchardCore.Email;
using OrchardCore.Email.Services;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
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
            var smtpService = CreateSmtpService(new SmtpSettings());
            var task = new EmailTask(
                smtpService,
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
                new List<ExecutedActivity>(),
                default,
                Enumerable.Empty<ActivityContext>());
            var activityContext = Mock.Of<ActivityContext>();

            // Act
            var result = await task.ExecuteAsync(executionContext, activityContext);

            // Assert
            Assert.Equal("Failed", result.Outcomes.First());
        }

        private static ISmtpService CreateSmtpService(SmtpSettings settings)
        {
            var secretService = GetSecretServiceMock();
            var options = new Mock<IOptions<SmtpSettings>>();
            var logger = new Mock<ILogger<SmtpService>>();
            var localizer = new Mock<IStringLocalizer<SmtpService>>();
            var smtp = new SmtpService(secretService, options.Object, logger.Object, localizer.Object);

            options.Setup(o => o.Value).Returns(settings);

            return smtp;
        }

        private static ISecretService GetSecretServiceMock()
        {
            var passwordSecret = new TextSecret()
            {
                Name = "OrchardCore.Email.Secrets.Password",
                Text = "email.password",
            };

            var passwordInfo = new SecretInfo() { Name = "OrchardCore.Email.Secrets.Password" };
            var secrets = new Dictionary<string, SecretInfo>()
            {
                { "OrchardCore.Email.Secrets.Password", passwordInfo },
            };

            var secretService = Mock.Of<ISecretService>();

            Mock.Get(secretService).Setup(s => s.GetSecretInfosAsync()).ReturnsAsync(secrets);
            Mock.Get(secretService).Setup(s => s.GetSecretAsync<TextSecret>(passwordSecret.Name)).ReturnsAsync(passwordSecret);

            return secretService;
        }

        private class SimpleWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
        {
            public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
                => await Task.FromResult((T)Convert.ChangeType(expression.Expression, typeof(T)));
        }
    }
}
