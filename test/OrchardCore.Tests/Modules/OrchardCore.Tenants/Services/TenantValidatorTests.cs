using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Modules.Tenants.Services.Tests
{
    public class TenantValidatorTests : SiteContext
    {
        static TenantValidatorTests()
        {
        }

        [Theory]
        [InlineData("Tenant1", "tenant1", "", "Feature Profile", new[] { "A tenant with the same name already exists." })]
        [InlineData("tEnAnT1", "tenant1", "", "Feature Profile", new[] { "A tenant with the same name already exists." })]
        [InlineData("dEfAuLt", "", "", "Feature Profile", new[] { "The tenant name is in conflict with the 'Default' tenant." })]
        [InlineData("Tenant5", "tenant3", "", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant5", "tenant3", null, "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant5", "", "example2.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant5", null, "example2.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant6", "tenant4", "example4.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("", "tenant7", "example1.com", "Feature Profile", new[] { "The tenant name is mandatory." })]
        [InlineData("Tenant7", "tenant7", "", "Feature", new[] { "The feature profile does not exist." })]
        [InlineData("@Invalid Tenant", "tenant7", "example1.com", "Feature Profile", new[] { "Invalid tenant name. Must contain characters only and no spaces." })]
        [InlineData("Tenant7", null, "  ", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant7", "/tenant7", "", "Feature Profile", new[] { "The url prefix can not contain more than one segment." })]
        [InlineData("@Invalid Tenant", "/tenant7", "", "Feature Profile", new[] { "Invalid tenant name. Must contain characters only and no spaces.", "The url prefix can not contain more than one segment." })]
        [InlineData("Tenant8", "tenant4", "example6.com,example4.com, example5.com", "Feature Profile", new[] { "A tenant with the same host and prefix already exists." })]
        [InlineData("Tenant9", "tenant9", "", "Feature Profile", new string[] { })]
        [InlineData("Tenant9", "", "example6.com", "Feature Profile", new string[] { })]
        [InlineData("Tenant9", "tenant9", "example6.com", "Feature Profile", new string[] { })]
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
                FeatureProfiles = new[] { featureProfile },
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
                FeatureProfiles = new[] { "Feature Profile" },
                IsNewTenant = isNewTenant
            };

            // Act
            var errors = await tenantValidator.ValidateAsync(viewModel);

            // Assert
            if (isNewTenant)
            {
                Assert.Single(errors);
                Assert.Equal("A tenant with the same host and prefix already exists.", errors.Single().Message);
            }
            else
            {
                Assert.Equal(2, errors.Count());
                Assert.Equal("A tenant with the same host and prefix already exists.", errors.ElementAt(0).Message);
                Assert.Equal("The existing tenant to be validated was not found.", errors.ElementAt(1).Message);
            }
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
                ? ShellHost.GetSettings(ShellSettings.DefaultShellName)
                : new ShellSettings();

            var dbConnectionValidatorMock = new Mock<IDbConnectionValidator>();
            var validationContext = new DbConnectionValidatorContext(shellSettings);

            dbConnectionValidatorMock.Setup(v => v.ValidateAsync(validationContext));

            return new TenantValidator(
                ShellHost,
                ShellSettingsManager,
                featureProfilesServiceMock.Object,
                dbConnectionValidatorMock.Object,
                stringLocalizerMock.Object
                );
        }

        private static async Task SeedTenantsAsync()
        {
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant1", RequestUrlPrefix = "tenant1" }.AsUninitialized());
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant2", RequestUrlPrefix = String.Empty, RequestUrlHost = "example2.com" }.AsUninitialized());
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant3", RequestUrlPrefix = "tenant3", RequestUrlHost = String.Empty }.AsUninitialized());
            await ShellHost.GetOrCreateShellContextAsync(new ShellSettings { Name = "Tenant4", RequestUrlPrefix = "tenant4", RequestUrlHost = "example4.com, example5.com" }.AsUninitialized());
        }
    }
}
