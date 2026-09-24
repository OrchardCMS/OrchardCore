using System.Text.Encodings.Web;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Modules.Tenants.Workflows.Tests;

public class CreateTenantTaskTests : SiteContext
{
    [Theory]
    [InlineData("..")]
    [InlineData("../..")]
    [InlineData("../../bin")]
    [InlineData("Invalid Tenant")]
    [InlineData("@Invalid")]
    public async Task ExecuteAsync_InvalidTenantName_FailsWithoutCreatingTheTenant(string tenantName)
    {
        // Arrange
        await ShellHost.InitializeAsync();

        var scope = await ShellHost.GetScopeAsync(ShellSettings.DefaultShellName);

        await scope.UsingAsync(async shellScope =>
        {
            var task = CreateTask(shellScope.ServiceProvider, tenantName, requestUrlPrefix: Guid.NewGuid().ToString("n"));

            using var workflowContext = CreateWorkflowContext();

            // Act
            var result = await task.ExecuteAsync(workflowContext, null);

            // Assert
            Assert.Equal("Failed", Assert.Single(result.Outcomes));
            Assert.False(ShellHost.TryGetSettings(tenantName, out _));
        });
    }

    [Fact]
    public async Task ExecuteAsync_ValidTenantName_CreatesTheTenant()
    {
        // Arrange
        await ShellHost.InitializeAsync();

        var tenantName = "Tenant" + Guid.NewGuid().ToString("n");
        var scope = await ShellHost.GetScopeAsync(ShellSettings.DefaultShellName);

        await scope.UsingAsync(async shellScope =>
        {
            var task = CreateTask(shellScope.ServiceProvider, tenantName, requestUrlPrefix: tenantName);

            using var workflowContext = CreateWorkflowContext();

            // Act
            var result = await task.ExecuteAsync(workflowContext, null);

            // Assert
            Assert.Equal("Done", Assert.Single(result.Outcomes));
            Assert.True(ShellHost.TryGetSettings(tenantName, out var settings));

            await ShellHost.RemoveShellSettingsAsync(settings);
        });
    }

    private static CreateTenantTask CreateTask(IServiceProvider serviceProvider, string tenantName, string requestUrlPrefix)
    {
        var expressionEvaluatorMock = new Mock<IWorkflowExpressionEvaluator>();

        expressionEvaluatorMock
            .Setup(e => e.EvaluateAsync(
                It.IsAny<WorkflowExpression<string>>(),
                It.IsAny<WorkflowExecutionContext>(),
                It.IsAny<TextEncoder>()))
            .Returns<WorkflowExpression<string>, WorkflowExecutionContext, TextEncoder>(
                (expression, _, _) => Task.FromResult(expression.Expression));

        var localizerMock = new Mock<IStringLocalizer<CreateTenantTask>>();

        localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns<string>(n => new LocalizedString(n, n));

        return new CreateTenantTask(
            serviceProvider.GetRequiredService<IShellSettingsManager>(),
            serviceProvider.GetRequiredService<IShellHost>(),
            expressionEvaluatorMock.Object,
            Mock.Of<IWorkflowScriptEvaluator>(),
            serviceProvider.GetRequiredService<ITenantValidator>(),
            localizerMock.Object)
        {
            TenantName = new WorkflowExpression<string>(tenantName),
            RequestUrlPrefix = new WorkflowExpression<string>(requestUrlPrefix),
        };
    }

    private static WorkflowExecutionContext CreateWorkflowContext()
        => new(new WorkflowType(), new Workflow(), null, null, null, null, null, []);
}
