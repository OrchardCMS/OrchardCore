using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.Drivers;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Modules.Tenants.Workflows.Tests;

public class CreateTenantTaskDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_NameAndDescription_StoresEachExpressionInItsOwnProperty()
    {
        // Arrange
        var updater = new Mock<IUpdateModel>();
        updater.Setup(model => model.TryUpdateModelAsync(It.IsAny<CreateTenantTaskViewModel>(), It.IsAny<string>()))
            .Callback<CreateTenantTaskViewModel, string>((model, prefix) =>
            {
                model.TenantNameExpression = "{{ Workflow.Input.Name }}";
                model.DescriptionExpression = "{{ Workflow.Input.Description }}";
            })
            .ReturnsAsync(true);

        var activity = CreateActivity();
        var driver = new CreateTenantTaskDisplayDriver();
        var context = new UpdateEditorContext(new Shape(), string.Empty, false, string.Empty, null, null, updater.Object);

        // Act
        await driver.UpdateAsync(activity, context);

        // Assert
        Assert.Equal("{{ Workflow.Input.Name }}", activity.TenantName.Expression);
        Assert.Equal("{{ Workflow.Input.Description }}", activity.Description.Expression);
    }

    private static CreateTenantTask CreateActivity()
    {
        var localizer = new Mock<IStringLocalizer<CreateTenantTask>>();
        localizer.Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        return new CreateTenantTask(
            Mock.Of<IShellSettingsManager>(),
            Mock.Of<IShellHost>(),
            Mock.Of<IWorkflowExpressionEvaluator>(),
            Mock.Of<IWorkflowScriptEvaluator>(),
            Mock.Of<ITenantValidator>(),
            Mock.Of<IStringLocalizerFactory>(),
            localizer.Object);
    }
}
