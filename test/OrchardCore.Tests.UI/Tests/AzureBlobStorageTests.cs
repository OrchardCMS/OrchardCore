using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
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

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithBlogAndAzureBlobStorage(Browser browser) =>
            ExecuteTestAsync(
                context => context.TestBasicOrchardFeaturesAsync("Blog.Tests"),
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
