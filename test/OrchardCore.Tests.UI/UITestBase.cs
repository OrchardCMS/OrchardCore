using System;
using System.Threading.Tasks;
using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Services;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI
{
    public class UITestBase : OrchardCoreUITestBase<Program>
    {
        protected UITestBase(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override Task ExecuteTestAfterSetupAsync(
            Func<UITestContext, Task> testAsync,
            Browser browser,
            Func<OrchardCoreUITestExecutorConfiguration, Task> changeConfigurationAsync)
        {
            throw new NotSupportedException(
                "Since these tests are run for all database engines supported by Orchard Core, and setup snapshotting " +
                "required by this method is only supported for SQLite and SQL Server by the UI Testing Toolbox, this " +
                "isn't supported. Run the applicable setup operation in the test instead.");
        }

        protected override Task ExecuteTestAsync(
            Func<UITestContext, Task> testAsync,
            Browser browser,
            Func<UITestContext, Task<Uri>> setupOperation,
            Func<OrchardCoreUITestExecutorConfiguration, Task> changeConfigurationAsync) =>
            base.ExecuteTestAsync(
                testAsync,
                browser,
                setupOperation,
                async configuration =>
                {
                    configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;

                    if (changeConfigurationAsync != null) await changeConfigurationAsync(configuration);
                });
    }
}
