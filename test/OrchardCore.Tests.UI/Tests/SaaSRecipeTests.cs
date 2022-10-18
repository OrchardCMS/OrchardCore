using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class SaaSRecipeTests : UITestBase
    {
        private const string TestTenantUrlPrefix = "test";
        private const string TestTenantDisplayName = "Test Tenant";

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
        public Task CreatingTenantShouldWork(Browser browser) =>
            ExecuteTestAfterSetupAsync(
                async context =>
                {
                    await context.CreateAndEnterTenantAsync(
                        TestTenantUrlPrefix,
                        TestTenantUrlPrefix,
                        "Blog.Tests",
                        new TenantSetupParameters
                        {
                            SiteName = TestTenantDisplayName,
                        });

                    Assert.Equal(TestTenantDisplayName, context.Get(By.ClassName("navbar-brand")).Text);

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
