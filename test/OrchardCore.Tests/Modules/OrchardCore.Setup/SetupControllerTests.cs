using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Controllers;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;

namespace OrchardCore.Modules.OrchardCore.Setup.Tests;

public class SetupControllerTests
{
    [Fact]
    public async Task IndexShouldKeepDatabaseOptionsVisible_WhenOnlyTablePrefixIsPreset()
    {
        // Arrange
        var controller = CreateController(new ShellSettings
        {
            ["TablePrefix"] = "Default",
        });

        // Act
        var result = await controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SetupViewModel>(viewResult.Model);
        Assert.False(model.DatabaseConfigurationPreset);
        Assert.Equal("Default", model.TablePrefix);
    }

    [Fact]
    public async Task IndexShouldHideDatabaseOptions_WhenDatabaseProviderOrConnectionIsPreset()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["DatabaseProvider"] = DatabaseProviderValue.SqlConnection;
        shellSettings["ConnectionString"] = "Server=localhost;Database=Orchard;";
        shellSettings["TablePrefix"] = "Default";

        var controller = CreateController(shellSettings);

        // Act
        var result = await controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SetupViewModel>(viewResult.Model);
        Assert.True(model.DatabaseProviderPreset);
        Assert.True(model.DatabaseConfigurationPreset);
        Assert.Equal("Default", model.TablePrefix);
    }

    [Fact]
    public async Task IndexShouldKeepConnectionOptionsVisible_WhenOnlyDatabaseProviderIsConfigured()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["DatabaseProvider"] = DatabaseProviderValue.SqlConnection;

        var controller = CreateController(shellSettings);

        // Act
        var result = await controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SetupViewModel>(viewResult.Model);
        Assert.True(model.DatabaseProviderPreset);
        Assert.False(model.DatabaseConfigurationPreset);
        Assert.Equal(DatabaseProviderValue.SqlConnection, model.DatabaseProvider);
    }

    [Fact]
    public async Task IndexPostShouldUsePresetDatabaseProviderAndPostedConnectionString_WhenOnlyDatabaseProviderIsConfigured()
    {
        // Arrange
        SetupContext capturedContext = null;
        var shellSettings = new ShellSettings();
        shellSettings["DatabaseProvider"] = DatabaseProviderValue.SqlConnection;

        var setupService = new Mock<ISetupService>();
        setupService
            .Setup(x => x.GetSetupRecipesAsync())
            .ReturnsAsync(
            [
                new RecipeDescriptor
                {
                    Name = "SaaS",
                    Tags = ["default"],
                },
            ]);
        setupService
            .Setup(x => x.SetupAsync(It.IsAny<SetupContext>()))
            .Callback<SetupContext>(context => capturedContext = context)
            .ReturnsAsync("execution-id");

        var controller = CreateController(shellSettings, setupService.Object);

        var model = new SetupViewModel
        {
            SiteName = "Tenant",
            UserName = "admin",
            Email = "admin@test.com",
            Password = "Password1!",
            PasswordConfirmation = "Password1!",
            RecipeName = "SaaS",
            DatabaseProvider = DatabaseProviderValue.Sqlite,
            ConnectionString = "Server=localhost;Database=Tenant;",
            SiteTimeZone = "UTC",
        };

        // Act
        var result = await controller.IndexPOST(model);

        // Assert
        Assert.IsType<RedirectResult>(result);
        Assert.NotNull(capturedContext);
        Assert.Equal(DatabaseProviderValue.SqlConnection, capturedContext.Properties[SetupConstants.DatabaseProvider]);
        Assert.Equal("Server=localhost;Database=Tenant;", capturedContext.Properties[SetupConstants.DatabaseConnectionString]);
    }

    private static SetupController CreateController(ShellSettings shellSettings, ISetupService setupService = null)
    {
        var recipes = new[]
        {
            new RecipeDescriptor
            {
                Name = "SaaS",
                Tags = ["default"],
            },
        };

        if (setupService is null)
        {
            var setupServiceMock = new Mock<ISetupService>();
            setupServiceMock
                .Setup(x => x.GetSetupRecipesAsync())
                .ReturnsAsync(recipes);
            setupServiceMock
                .Setup(x => x.SetupAsync(It.IsAny<SetupContext>()))
                .ReturnsAsync("execution-id");

            setupService = setupServiceMock.Object;
        }

        var emailAddressValidator = new Mock<IEmailAddressValidator>();
        emailAddressValidator
            .Setup(x => x.Validate(It.IsAny<string>()))
            .Returns(true);

        return new SetupController(
            Mock.Of<IClock>(),
            setupService,
            shellSettings,
            Mock.Of<IShellHost>(),
            Options.Create(new IdentityOptions()),
            emailAddressValidator.Object,
            [
                new DatabaseProvider
                {
                    Name = "Sqlite",
                    Value = DatabaseProviderValue.Sqlite,
                    IsDefault = true,
                },
                new DatabaseProvider
                {
                    Name = "Sql Server",
                    Value = DatabaseProviderValue.SqlConnection,
                    HasConnectionString = true,
                    HasTablePrefix = true,
                },
            ],
            Mock.Of<IStringLocalizer<SetupController>>(),
            Mock.Of<ILogger<SetupController>>());
    }
}
