using System.Linq;
using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class BlankRecipeTests : UITestBase
    {
        public BlankRecipeTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithBlank(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.TestSetupAsync(new OrchardCoreSetupParameters(context)
                    {
                        RecipeId = "Blank.Tests",
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

                    configuration.AssertBrowserLog = logEntries =>
                        logEntries.ShouldNotContain(
                            logEntry => IsValidBrowserLogEntry(logEntry),
                            logEntries.Where(IsValidBrowserLogEntry).ToFormattedString());

                    return Task.CompletedTask;
                });

        private static bool IsValidBrowserLogEntry(LogEntry logEntry)
        {
            return OrchardCoreUITestExecutorConfiguration.IsValidBrowserLogEntry(logEntry) &&
                !logEntry.IsNotFoundLogEntry("/");
        }
    }
}
