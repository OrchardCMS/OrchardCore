using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;
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
                context => context
                    .TestBasicOrchardFeaturesExceptRegistrationAsync(
                        new OrchardCoreSetupParameters(context)
                        {
                            RecipeId = "Lombiq.OSOCE.NuGet.BasicOrchardFeaturesTests",
                        }),
                browser);
    }
}
