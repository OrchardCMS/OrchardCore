using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class SaaSRecipeTests : UITestBase
    {
        public SaaSRecipeTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithSaaS(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.TestSetupWithInvalidAndValidDataAsync(new OrchardCoreSetupParameters(context)
                    {
                        RecipeId = "SaaS.Tests",
                    });

                    await context.TestRegistrationWithInvalidDataAsync();
                    await context.TestRegistrationAsync();
                    await context.TestRegistrationWithAlreadyRegisteredEmailAsync();
                    await context.TestLoginWithInvalidDataAsync();
                    await context.TestLoginAsync();
                    await context.TestTurningFeatureOnAndOffAsync();
                    await context.TestLogoutAsync();
                },
                browser);

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithNewTenant(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.GoToSetupPageAndSetupOrchardCoreAsync(
                       new OrchardCoreSetupParameters(context)
                       {
                           SiteName = "Orchard Core - UI Testing",
                           RecipeId = "SaaS.Tests",
                           TablePrefix = "default",
                           SiteTimeZoneValue = "America/New_York",
                       });

                    await context.SignInDirectlyAsync();

                    await context.CreateAndSwitchToTenantManuallyAsync("test", "test");

                    await context.GoToSetupPageAndSetupOrchardCoreAsync(
                        new OrchardCoreSetupParameters(context)
                        {
                            SiteName = "Test Tenant",
                            RecipeId = "Blog.Tests",
                            TablePrefix = "test",
                            RunSetupOnCurrentPage = true,
                        });

                    await context.TestBasicOrchardFeaturesExceptSetupAsync();
                },
                browser,
                configuration =>
                {
                    configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                        AccessibilityCheckingConfiguration
                            .ConfigureWcag21aa(axeBuilder)
                            .DisableRules("color-contrast");

                    return Task.CompletedTask;
                });
    }
}
