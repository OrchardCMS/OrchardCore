using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Data;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.ViewModels;
using Xunit;

namespace OrchardCore.Modules.Tenants.Tests
{
    public class AdminControllerTests
    {
        private readonly IList<ShellSettings> _shellSettings = new List<ShellSettings>();

        public AdminControllerTests() => SeedTenants();

        [Theory]
        [InlineData("Tenant3", "", "example1.com", false)]
        [InlineData("Tenant4", "", "example2.com", false)]
        [InlineData("Tenant5", "tenant1", "example1.com", false)]
        [InlineData("Tenant6", "tenant1", "example2.com", true)]
        [InlineData("Tenant7", "tenant2", "example2.com", true)]
        public async Task CreateTenant(string name, string urlPrefix, string hostname, bool isValid)
        {
            // Arrange
            var shellHostMock = new Mock<IShellHost>();
            shellHostMock.Setup(h => h.GetAllSettings()).Returns(_shellSettings);
            shellHostMock.Setup(h => h
                .UpdateShellSettingsAsync(It.IsAny<ShellSettings>()))
                .Callback<ShellSettings>(s => _shellSettings.Add(s));

            var shellSettingsManagerMock = new Mock<IShellSettingsManager>();
            shellSettingsManagerMock.Setup(sm => sm.CreateDefaultSettings()).Returns(new ShellSettings());

            var authServiceMock = new Mock<IAuthorizationService>(MockBehavior.Strict);

            authServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            authServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var featureProfilesServiceMock = new Mock<IFeatureProfilesService>();
            featureProfilesServiceMock.Setup(fp => fp.GetFeatureProfilesAsync())
                .Returns(Task.FromResult((IDictionary<string, FeatureProfile>)new Dictionary<string, FeatureProfile>
                {
                    { "Feature Profile", new FeatureProfile() }
                }));

            var stringLocalizerMock = new Mock<IStringLocalizer<AdminController>>();
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(n => new LocalizedString(n, n));
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns<string, object[]>((n, a) => new LocalizedString(n, n));

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Loose);
            httpContextMock.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

            var controller = new AdminController(
                shellHostMock.Object,
                shellSettingsManagerMock.Object,
                Enumerable.Empty<DatabaseProvider>(),
                authServiceMock.Object,
                _shellSettings.First(),
                featureProfilesServiceMock.Object,
                Enumerable.Empty<IRecipeHarvester>(),
                Mock.Of<IDataProtectionProvider>(),
                new Clock(),
                Mock.Of<INotifier>(),
                Mock.Of<ISiteService>(),
                Mock.Of<IShapeFactory>(),
                stringLocalizerMock.Object,
                Mock.Of<IHtmlLocalizer<AdminController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };
            var viewModel = new EditTenantViewModel
            {
                Name = name,
                RequestUrlPrefix = urlPrefix,
                RequestUrlHost = hostname,
                FeatureProfile = "Feature Profile"
            };

            // Act
            var result = await controller.Create(viewModel);

            // Assert
            Assert.Equal(isValid, controller.ModelState.IsValid);

            if (!isValid)
            {
                Assert.Equal("A tenant with the same host and prefix already exists.", controller.ModelState.First().Value.Errors.First().ErrorMessage);
            }
        }

        private void SeedTenants()
        {
            _shellSettings.Add(new ShellSettings
            {
                Name = ShellHelper.DefaultShellName
            });
            _shellSettings.Add(new ShellSettings
            {
                Name = "Tenant1",
                RequestUrlPrefix = "tenant1",
                RequestUrlHost = "example1.com"
            });
            _shellSettings.Add(new ShellSettings
            {
                Name = "Tenant2",
                RequestUrlPrefix = "",
                RequestUrlHost = "example1.com,example2.com"
            });
        }
    }
}
