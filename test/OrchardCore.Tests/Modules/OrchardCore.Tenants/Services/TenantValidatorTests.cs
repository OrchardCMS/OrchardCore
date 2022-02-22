using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;
using Xunit;

namespace OrchardCore.Modules.Tenants.Services.Tests
{
    public class TenantValidatorTests
    {
        private readonly IList<ShellSettings> _shellSettings = new List<ShellSettings>();

        public TenantValidatorTests() => SeedTenants();

        [Theory]
        [InlineData("", "tenant1", "example1.com", "Feature Profile", new[] { "The tenant name is mandatory." })]
        [InlineData("Tenant1", "tenant1", "", "Feature", new[] { "The feature profile does not exist." })]
        [InlineData("@Invalid Tenant", "tenant1", "example1.com", "Feature Profile", new[] { "Invalid tenant name. Must contain characters only and no spaces." })]
        [InlineData("Tenant1", null, "  ", "Feature Profile", new[] { "Host and url prefix can not be empty at the same time." })]
        [InlineData("Tenant1", "/tenant1", "", "Feature Profile", new[] { "The url prefix can not contain more than one segment." })]
        [InlineData("@Invalid Tenant", "/tenant1", "", "Feature Profile", new[] { "Invalid tenant name. Must contain characters only and no spaces.", "The url prefix can not contain more than one segment." })]
        public async Task ExamineInvalidTenantErrors(string name, string urlPrefix, string hostName, string featureProfile, string[] errorMessages)
        {
            // Arrange
            var tenantValidator = CreateTenantValidator(defaultTenant: false);

            // Act & Assert
            var viewModel = new EditTenantViewModel
            {
                Name = name,
                RequestUrlPrefix = urlPrefix,
                RequestUrlHost = hostName,
                FeatureProfile = featureProfile
            };

            var errors = await tenantValidator.ValidateAsync(viewModel);

            // Assert
            Assert.Equal(errorMessages.Length, errors.Count());

            for (var i = 0; i < errors.Count(); i++)
            {
                Assert.Equal(errorMessages[i], errors.ElementAt(i).Message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldNotCreateTenantAlreadyUsedInMultipleHostnames(bool isNewTenant)
        {
            // Arrange
            var tenantValidator = CreateTenantValidator();

            var viewModel = new EditTenantViewModel
            {
                Name = "Tenant2",
                RequestUrlHost = "example2.com",
                FeatureProfile = "Feature Profile",
                IsNewTenant = isNewTenant
            };

            _shellSettings.Add(new ShellSettings
            {
                Name = "Tenant1",
                RequestUrlHost = "exmple1.com, example2.com"
            });

            // Act
            var errors = await tenantValidator.ValidateAsync(viewModel);

            // Asserts
            Assert.Single(errors);
            Assert.Equal("A tenant with the same host and prefix already exists.", errors.Single().Message);
        }

        private TenantValidator CreateTenantValidator(bool defaultTenant = true)
        {
            var shellHostMock = new Mock<IShellHost>();
            shellHostMock.Setup(h => h.GetAllSettings()).Returns(_shellSettings);

            var featureProfilesServiceMock = new Mock<IFeatureProfilesService>();
            featureProfilesServiceMock.Setup(fp => fp.GetFeatureProfilesAsync())
                .Returns(Task.FromResult((IDictionary<string, FeatureProfile>)new Dictionary<string, FeatureProfile>
                {
                    { "Feature Profile", new FeatureProfile() }
                }));

            var stringLocalizerMock = new Mock<IStringLocalizer<TenantValidator>>();
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(n => new LocalizedString(n, n));
            stringLocalizerMock
                .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns<string, object[]>((n, a) => new LocalizedString(n, n));

            var shellSettings = defaultTenant
                ? _shellSettings.First()
                : new ShellSettings();

            return new TenantValidator(
                shellHostMock.Object,
                featureProfilesServiceMock.Object,
                Enumerable.Empty<DatabaseProvider>(),
                shellSettings,
                stringLocalizerMock.Object);
        }

        private void SeedTenants()
        {
            _shellSettings.Add(new ShellSettings
            {
                Name = ShellHelper.DefaultShellName
            });
        }
    }
}
