using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Modules.Tenants.Services.Tests
{
    public class TenantValidatorTests : SiteContext
    {
        public static IShellHost ShellHost { get; }

        static TenantValidatorTests()
        {
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        [Theory]
        [InlineData("Tenant1", "tenant1", "", "Feature Profile", new[] { "A tenant with the same name already exists." })]
        [InlineData("tEnAnT1", "tenant1", "", "Feature Profile", new[] { "A tenant with the same name already exists." })]
        [InlineData("dEfAuLt", "", "", "Feature Profile", new[] { "The tenant name is in conflict with the 'Default' tenant." })]
        [InlineData("Tenant5", "tenant3", "", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant5", "tenant3", null, "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant5", "", "example2.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant6", "tenant4", "example4.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("", "tenant7", "example1.com", "Feature Profile", new[] { "The tenant name is mandatory." })]
        [InlineData("Tenant7", "tenant7", "", "Feature", new[] { "The feature profile does not exist." })]
        [InlineData("@Invalid Tenant", "tenant7", "example1.com", "Feature Profile", new[] { "Invalid tenant name. Must contain characters only and no spaces." })]
        [InlineData("Tenant7", null, "  ", "Feature Profile", new[] { "Host and url prefix can not be empty at the same time." })]
        [InlineData("Tenant7", "/tenant7", "", "Feature Profile", new[] { "The url prefix can not contain more than one segment." })]
        [InlineData("@Invalid Tenant", "/tenant7", "", "Feature Profile", new[] { "Invalid tenant name. Must contain characters only and no spaces.", "The url prefix can not contain more than one segment." })]
        [InlineData("Tenant8", "tenant4", "example6.com,example4.com, example5.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant9", "tenant9", "", "Feature Profile", new string[] { })]
        [InlineData("Tenant9", "", "example6.com", "Feature Profile", new string[] { })]
        [InlineData("Tenant9", "tenant9", "example6.com", "Feature Profile", new string[] { })]
        [InlineData("Tenant9", null, "example2.com", "Feature Profile", new string[] { })]
        public async Task TenantValidationFailsIfInvalidConfigurationsWasProvided(string name, string urlPrefix, string hostName, string featureProfile, string[] errorMessages)
        {
            // Arrange
            await ShellHost.InitializeAsync();
            await SeedTenantsAsync();

            var tenantValidator = CreateTenantValidator(defaultTenant: false);

            // Act
            var viewModel = new EditTenantViewModel
            {
                Name = name,
                RequestUrlPrefix = urlPrefix,
                RequestUrlHost = hostName,
                FeatureProfile = featureProfile,
                IsNewTenant = true
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
        public async Task DuplicateTenantHostOrPrefixShouldFailValidation(bool isNewTenant)
        {
            // Arrange
            await ShellHost.InitializeAsync();
            await SeedTenantsAsync();

            var tenantValidator = CreateTenantValidator();

            var viewModel = new EditTenantViewModel
            {
                Name = "Tenant5",
                RequestUrlPrefix = "tenant4",
                RequestUrlHost = "example5.com",
                FeatureProfile = "Feature Profile",
                IsNewTenant = isNewTenant
            };

            // Act
            var errors = await tenantValidator.ValidateAsync(viewModel);

            // Assert
            Assert.Single(errors);
            Assert.Equal("A tenant with the same host and prefix already exists.", errors.Single().Message);
        }

        private static TenantValidator CreateTenantValidator(bool defaultTenant = true)
        {
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
                ? ShellHost.GetSettings(ShellHelper.DefaultShellName)
                : new ShellSettings();

            var connectionFactory = new Mock<IDbConnectionValidator>();
            connectionFactory.Setup(l => l.ValidateAsync(shellSettings["ProviderName"], shellSettings["ConnectionName"], shellSettings["TablePrefix"], shellSettings.Name));

            return new TenantValidator(
                ShellHost,
                featureProfilesServiceMock.Object,
                connectionFactory.Object,
                stringLocalizerMock.Object
                );
        }

        private static async Task SeedTenantsAsync()
        {
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant1", State = TenantState.Uninitialized });
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant2", State = TenantState.Uninitialized, RequestUrlPrefix = String.Empty, RequestUrlHost = "example2.com" });
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant2", State = TenantState.Uninitialized, RequestUrlPrefix = String.Empty, RequestUrlHost = "example2.com" });
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant3", State = TenantState.Uninitialized, RequestUrlPrefix = "tenant3", RequestUrlHost = String.Empty });
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant4", State = TenantState.Uninitialized, RequestUrlPrefix = "tenant4", RequestUrlHost = "example4.com,example5.com" });
        }
    }
}
