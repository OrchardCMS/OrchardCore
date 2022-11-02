using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class AzureBlobStorageTests : UITestBase
    {
        public AzureBlobStorageTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory(Skip = "Minimal test suite for multi-DB testing."), Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithBlogAndAzureBlobStorage(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.TestSetupAsync(new OrchardCoreSetupParameters(context)
                    {
                        RecipeId = "Blog.Tests",
                    }.DatabaseProviderFromEnvironmentIfAvailable(context));

                    await context.TestBasicOrchardFeaturesExceptSetupAsync();
                },
                browser,
                configuration =>
                {
                    configuration.UseAzureBlobStorage = true;

                    configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                        AccessibilityCheckingConfiguration
                            .ConfigureWcag21aa(axeBuilder)
                            .DisableRules("color-contrast");

                    return Task.CompletedTask;
                });
    }
}
