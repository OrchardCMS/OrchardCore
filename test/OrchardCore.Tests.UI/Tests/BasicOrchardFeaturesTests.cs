using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using OrchardCore.Tests.UI.Extensions;
using OrchardCore.Tests.UI.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class BasicOrchardFeaturesTests : UITestBase
    {
        public BasicOrchardFeaturesTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWork(Browser browser) =>
            ExecuteTestAsync(
                context => context.TestBasicOrchardFeaturesExceptRegistrationAsync(SetupHelpers.RecipeId),
                browser,
                configuration =>
                {
                    configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
                    configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                        AccessibilityCheckingConfiguration
                            .ConfigureWcag21aa(axeBuilder)
                            .DisableRules("color-contrast");

                    return Task.CompletedTask;
                });

        [Theory(Skip = "Used to test artifact creation during build."), Chrome]
        public Task IntentionallyFailingTest(Browser browser) =>
            ExecuteTestAfterSetupAsync(
                context => context.Exists(By.Id("navbarasdfds")),
                browser);
    }
}
