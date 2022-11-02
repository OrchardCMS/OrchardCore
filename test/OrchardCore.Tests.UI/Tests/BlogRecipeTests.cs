using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class BlogRecipeTests : UITestBase
    {
        public BlogRecipeTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithBlog(Browser browser) =>
            ExecuteTestAsync(
                context => context.TestBasicOrchardFeaturesAsync(new OrchardCoreSetupParameters(context)
                {
                    RecipeId = "Blog.Tests",
                }.DatabaseProviderFromEnvironmentIfAvailable(context)),
                browser,
                configuration =>
                {
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
