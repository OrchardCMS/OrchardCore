using System;
using System.Linq;
using System.Threading.Tasks;
using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.MonkeyTesting;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class AdminMonkeyTests : UITestBase
    {
        public AdminMonkeyTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory(Skip = "Minimal test suite for multi-DB testing."), Chrome]
        public Task TestAdminPagesAsMonkeyRecursivelyShouldWorkWithAdminUser(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.GoToSetupPageAndSetupOrchardCoreAsync(
                       new OrchardCoreSetupParameters(context)
                       {
                           SiteName = "Orchard Core - UI Testing",
                           RecipeId = "Blog.Tests",
                           SiteTimeZoneValue = "America/New_York",
                       }.DatabaseProviderFromEnvironmentIfAvailable(context));

                    await context.TestAdminAsMonkeyRecursivelyAsync(
                        new MonkeyTestingOptions
                        {
                            PageTestTime = TimeSpan.FromSeconds(10)
                        });
                },
                browser,
                configuration =>
                {
                    configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = false;

                    // This is necessary to work around this bug: https://github.com/OrchardCMS/OrchardCore/issues/11420.
                    configuration.AssertBrowserLog = logEntries => logEntries.ShouldNotContain(
                        logEntry => IsValidAdminBrowserLogEntry(logEntry),
                        logEntries.Where(IsValidAdminBrowserLogEntry).ToFormattedString());

                    return Task.CompletedTask;
                });

        private static bool IsValidAdminBrowserLogEntry(LogEntry logEntry)
        {
            return OrchardCoreUITestExecutorConfiguration.IsValidBrowserLogEntry(logEntry) &&
                !logEntry.Message.ContainsOrdinalIgnoreCase(
                    "Blocked attempt to show a 'beforeunload' confirmation panel for a frame that never had a user gesture since its load.");
        }
    }
}
