using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
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

namespace OrchardCore.Modules.Tenants.Controllers.Tests
{
    public class AdminControllerTests
    {
        private readonly IList<ShellSettings> _shellSettings = new List<ShellSettings>();
        private readonly Mock<IShellHost> _shellHostMock;
        private readonly Mock<IFeatureProfilesService> _featureProfilesServiceMock;

        public AdminControllerTests()
        {
            SeedTenants();

            _shellHostMock = new Mock<IShellHost>();
            _shellHostMock.Setup(h => h.GetAllSettings()).Returns(_shellSettings);
            _shellHostMock.Setup(h => h
                .UpdateShellSettingsAsync(It.IsAny<ShellSettings>()))
                .Callback<ShellSettings>(s => _shellSettings.Add(s));

            _featureProfilesServiceMock = new Mock<IFeatureProfilesService>();
            _featureProfilesServiceMock.Setup(fp => fp.GetFeatureProfilesAsync())
                .Returns(Task.FromResult((IDictionary<string, FeatureProfile>)new Dictionary<string, FeatureProfile>
                {
                    { "Feature Profile", new FeatureProfile() }
                }));
        }

        [Fact]
        public async Task CreateTenantShouldChecksHostnameIfItAlreadyUsedInMultipleHostnames()
        {
            // Arrange
            var shellSettingsManagerMock = new Mock<IShellSettingsManager>();
            shellSettingsManagerMock.Setup(sm => sm.CreateDefaultSettings()).Returns(new ShellSettings());

            var authServiceMock = new Mock<IAuthorizationService>(MockBehavior.Strict);

            authServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            authServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var stringLocalizerMock = new Mock<IStringLocalizer<AdminController>>();
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(n => new LocalizedString(n, n));
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns<string, object[]>((n, a) => new LocalizedString(n, n));

            var controller = new AdminController(
                _shellHostMock.Object,
                shellSettingsManagerMock.Object,
                Enumerable.Empty<DatabaseProvider>(),
                authServiceMock.Object,
                _featureProfilesServiceMock.Object,
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
                    HttpContext = CreateHttpContext()
                }
            };

            // Act & Assert
            var result = await CreateTenantAsync(controller, "Tenant1", "example1.com, example2.com");

            Assert.True(controller.ModelState.IsValid);

            result = await CreateTenantAsync(controller, "Tenant2", "example1.com");

            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("A tenant with the same host and prefix already exists.", controller.ModelState.First().Value.Errors.First().ErrorMessage);
        }

        private async static Task<IActionResult> CreateTenantAsync(AdminController controller, string name, string url)
        {
            var viewModel = new EditTenantViewModel
            {
                Name = name,
                RequestUrlHost = url,
                FeatureProfile = "Feature Profile"
            };

            return await controller.Create(viewModel);
        }

        private void SeedTenants()
        {
            _shellSettings.Add(new ShellSettings
            {
                Name = ShellHelper.DefaultShellName
            });
        }

        private HttpContext CreateHttpContext()
        {
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Loose);
            httpContextMock.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());
            httpContextMock.SetupGet(hc => hc.Items).Returns(new Dictionary<object, object>());
            httpContextMock.SetupGet(hc => hc.RequestServices).Returns(CreateServices());

            return httpContextMock.Object;
        }

        private IServiceProvider CreateServices()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var urlHelperFactoryMock = Mock.Of<IUrlHelperFactory>();
            var stringLocalizerMock = new Mock<IStringLocalizer>();

            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(n => new LocalizedString(n, n));
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns<string, object[]>((n, a) => new LocalizedString(n, n));

            serviceProvider
                .Setup(x => x.GetService(typeof(ShellSettings)))
                .Returns(_shellSettings.First());
            serviceProvider
                .Setup(x => x.GetService(typeof(IShellHost)))
                .Returns(_shellHostMock.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IFeatureProfilesService)))
                .Returns(_featureProfilesServiceMock.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<DatabaseProvider>)))
                .Returns(Enumerable.Empty<DatabaseProvider>());
            serviceProvider
                .Setup(x => x.GetService(typeof(IStringLocalizer)))
                .Returns(stringLocalizerMock.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock);
            serviceProvider
                .Setup(x => x.GetService(typeof(IActionResultExecutor<RedirectToPageResult>)))
                .Returns(new RedirectToPageResultExecutor(NullLoggerFactory.Instance, urlHelperFactoryMock));
            serviceProvider
                .Setup(x => x.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(new TempDataDictionaryFactory(Mock.Of<ITempDataProvider>()));
            //urlHelperFactoryMock.Setup(uf => uf.GetUrlHelper(It.IsAny<ActionContext>())).Returns<ActionContext>(c => new UrlHelper(new ActionContext()));

            return serviceProvider.Object;
        }
    }
}
