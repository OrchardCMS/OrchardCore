using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class InvalidSetupTests : UITestBase
    {
        public InvalidSetupTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task SetupWithInvalidDataShouldFail(Browser browser) =>
            ExecuteTestAsync(
                context => context.TestSetupWithInvalidDataAsync(
                    new OrchardCoreSetupParameters(context).ConfigureDatabaseSettings(context)),
                browser);
    }
}

