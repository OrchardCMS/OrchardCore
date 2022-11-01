using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class HeadlessRecipeTests : UITestBase
    {
        public HeadlessRecipeTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory(Skip = "Minimal test suite for multi-DB testing."), Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithHeadless(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.TestSetupAsync(new OrchardCoreSetupParameters(context)
                    {
                        RecipeId = "Headless.Tests",
                    });

                    await context.TestLoginWithInvalidDataAsync();
                    await context.TestLoginAsync();
                    await context.TestTurningFeatureOnAndOffAsync();
                    await context.TestLogoutAsync();
                },
                browser,
                configuration =>
                {
                    configuration.HtmlValidationConfiguration.RunHtmlValidationAssertionOnAllPageChanges = false;
                    configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = false;
                    return Task.CompletedTask;
                });
    }
}
