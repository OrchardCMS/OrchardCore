using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Modules.OrchardCore.Tenants.Tests;

public class ApiControllerTests
{
    private readonly Dictionary<string, ShellSettings> _shellSettings = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly Dictionary<string, FeatureProfile> _featureProfiles = new()
    {
        { "Feature Profile", new FeatureProfile() }
    };

    private delegate void TryGetSettingsCallback(string name, out ShellSettings settings);

    [Fact]
    public async Task CallCreateApiMultipleTimes_ShouldCreateTenant_ReturnsSetupToken()
    {
        // Arrange
        var controller = CreateController();
        var viewModel = new TenantApiModel
        {
            Name = "Test",
            RequestUrlPrefix = "test",
            RequestUrlHost = "orchardcore.net",
            FeatureProfiles = new[] { "Feature Profile" },
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

    [Fact]
    public async Task ShouldGetNewTenantSetupToken_WhenThePreviousTokenExpired()
    {
        // Arrange
        var controller = CreateController();
        var viewModel = new TenantApiModel
        {
            Name = "Test",
            RequestUrlPrefix = "test",
            RequestUrlHost = "orchardcore.net",
            FeatureProfiles = new[] { "Feature Profile" },
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

        // Enforce the setup token to be expired.
        _clockMock.Setup(clock => clock.UtcNow).Returns(DateTime.Now.AddDays(2));

        controller = CreateController();

        result = await controller.Create(viewModel);

        Assert.True(controller.ModelState.IsValid);
        Assert.IsType<CreatedResult>(result);

        var token2 = (result as CreatedResult).Location
            .ToString()
            .Split("token=")?[1];

        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
    }

    private ApiController CreateController()
    {
        var defaultShellSettings = new ShellSettings()
            .AsDefaultShell()
            .AsRunning();
            
        var shellHostMock = new Mock<IShellHost>();
        shellHostMock
            .Setup(host => host.UpdateShellSettingsAsync(It.IsAny<ShellSettings>()))
            .Callback<ShellSettings>(settings => _shellSettings.Add(settings.Name, settings));

        var _ = It.IsAny<ShellSettings>();
        shellHostMock
            .Setup(host => host.TryGetSettings(It.IsAny<string>(), out _))
            .Callback(new TryGetSettingsCallback((string name, out ShellSettings settings) => _shellSettings.TryGetValue(name, out settings)))
            .Returns<string, ShellSettings>((name, _) => _shellSettings.ContainsKey(name));

        var shellSettingsManagerMock = new Mock<IShellSettingsManager>();
        shellSettingsManagerMock
            .Setup(shellSettingsManager => shellSettingsManager.CreateDefaultSettings())
            .Returns(defaultShellSettings);

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

        var featureProfilesService = new Mock<IFeatureProfilesService>();
        featureProfilesService
            .Setup(featureProfiles => featureProfiles.GetFeatureProfilesAsync())
            .ReturnsAsync(_featureProfiles);

        var stringLocalizerMock = new Mock<IStringLocalizer<TenantValidator>>();
        stringLocalizerMock
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var tenantValidator = new TenantValidator(
            shellHostMock.Object,
            shellSettingsManagerMock.Object,
            featureProfilesService.Object,
            Mock.Of<IDbConnectionValidator>(),
            stringLocalizerMock.Object);

        return new ApiController(
            shellHostMock.Object,
            defaultShellSettings,
            Mock.Of<IShellRemovalManager>(),
            authorizeServiceMock.Object,
            shellSettingsManagerMock.Object,
            dataProtectionProviderMock.Object,
            Mock.Of<ISetupService>(),
            _clockMock.Object,
            Mock.Of<IEmailAddressValidator>(),
            Options.Create(new IdentityOptions()),
            Options.Create(new TenantsOptions()),
            Enumerable.Empty<DatabaseProvider>(),
            tenantValidator,
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
            OriginalPathBase = PathString.Empty
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
