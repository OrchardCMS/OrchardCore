using System;
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

        [Theory(Skip = "Minimal test suite for multi-DB testing."), Chrome]
        public Task SetupWithInvalidDataShouldFail(Browser browser) =>
            ExecuteTestAsync(
                context => context.TestSetupWithInvalidDataAsync(
                    new OrchardCoreSetupParameters(context)
                    {
                        SiteName = String.Empty,
                        UserName = String.Empty,
                        Email = String.Empty,
                        Password = String.Empty,
                    }.ConfigureDatabaseSettings(context)),
                browser);
    }
}

