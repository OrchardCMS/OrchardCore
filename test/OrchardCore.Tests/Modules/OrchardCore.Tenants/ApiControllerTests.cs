using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Modules.OrchardCore.Tenants.Tests;

public class ApiControllerTests
{
    private static Dictionary<string, ShellSettings> _shellSettings = new();

    [Fact]
    public async Task ShouldGetTenantSetupToken_AfterCreateTenantApiCalled()
    {
        // Arrange
        var controller = CreateController();
        var viewModel = new CreateApiViewModel
        {
            Name = "Test",
            RequestUrlPrefix = "/test",
            RequestUrlHost = "orchardcore.net",
            FeatureProfile = "Feature Profile",
            IsNewTenant = true
        };

        // Act & Assert
        var result = await controller.Create(viewModel);

        Assert.True(controller.ModelState.IsValid);
        Assert.IsType<OkObjectResult>(result);

        var token1 = (result as OkObjectResult).Value
            .ToString()
            .Split("token=")?[1];

        Assert.NotNull(token1);

        controller = CreateController();

        result = await controller.Create(viewModel);

        Assert.True(controller.ModelState.IsValid);
        Assert.IsType<CreatedResult>(result);

        var token2 = (result as CreatedResult).Location
            .ToString()
            .Split("token=")?[1];

        Assert.NotNull(token2);
        Assert.Equal(token1, token2);
    }

    private static ApiController CreateController()
    {
        var defaultShellSettings = new ShellSettings
        {
            Name = ShellHelper.DefaultShellName,
            State = TenantState.Running
        };
        ShellSettings settings;

        var shellHostMock = new Mock<IShellHost>();
        shellHostMock
            .Setup(host => host.UpdateShellSettingsAsync(It.IsAny<ShellSettings>()))
            .Callback<ShellSettings>(settings => _shellSettings.Add(settings.Name, settings));

        shellHostMock
            .Setup(host => host.TryGetSettings(It.IsAny<string>(), out settings))
            .Returns<string, ShellSettings>((name, settings) =>
            {
                var found = _shellSettings.ContainsKey(name);

                settings = found
                   ? _shellSettings[name]
                   : null;

                return found;
            });

        var shellSettingsManagerMock = new Mock<IShellSettingsManager>();
        shellSettingsManagerMock
            .Setup(shellSettingsManager => shellSettingsManager.CreateDefaultSettings())
            .Returns(defaultShellSettings);

        shellSettingsManagerMock
            .Setup(shellSettingsManager => shellSettingsManager.SaveSettingsAsync(It.IsAny<ShellSettings>()))
            .Callback<ShellSettings>(settings => _shellSettings.Add(settings.Name, settings));

        var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
        dataProtectionProviderMock
            .Setup(dataProtectionProvider => dataProtectionProvider.CreateProtector(It.IsAny<string>()))
            .Returns(new FakeDataProtector());

        var authorizeServiceMock = new Mock<IAuthorizationService>();
        authorizeServiceMock
            .Setup(authorizeService => authorizeService.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success);

        return new ApiController(
            shellHostMock.Object,
            defaultShellSettings,
            Mock.Of<IShellRemovalManager>(),
            authorizeServiceMock.Object,
            shellSettingsManagerMock.Object,
            dataProtectionProviderMock.Object,
            Mock.Of<ISetupService>(),
            Mock.Of<IClock>(),
            Mock.Of<IEmailAddressValidator>(),
            Options.Create(new IdentityOptions()),
            Options.Create(new TenantsOptions()),
            Enumerable.Empty<DatabaseProvider>(),
            Mock.Of<ITenantValidator>(),
            Mock.Of<IStringLocalizer<ApiController>>(),
            Mock.Of<ILogger<ApiController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
    }

    private static HttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal()
        };

        httpContext.Features.Set(new ShellContextFeature
        {
            OriginalPathBase = new PathString()
        });

        httpContext.User.AddIdentity(new ClaimsIdentity());

        return httpContext;
    }

    private class FakeDataProtector : IDataProtector
    {
        public IDataProtector CreateProtector(string purpose) => this;

        public byte[] Protect(byte[] plaintext) => plaintext;

        public byte[] Unprotect(byte[] protectedData) => protectedData;
    }
}
