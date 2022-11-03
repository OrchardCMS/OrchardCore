using System;
using System.IO;
using System.Threading.Tasks;
using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Configuration;
using OrchardCore.Tests.UI.Helpers;
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

                    configuration.UseSqlServer = TestConfigurationManager.RootConfiguration
                        .GetSection("OrchardCore")
                        .GetValue<bool>("UseSqlServerForUITesting");

                    if (configuration.UseSqlServer)
                    {
                        configuration.SqlServerDatabaseConfiguration.ConnectionStringTemplate =
                            configuration.SqlServerDatabaseConfiguration.ConnectionStringTemplate +
                            ";Encrypt=False;TrustServerCertificate=True";
                    }

                    configuration.OrchardCoreConfiguration.BeforeAppStart +=
                        (contentRootPath, argumentsBuilder) =>
                        {
                            // When DatabaseProvider comes from the environment then TablePrefix needs to, too, since
                            // the setup screen won't have a corresponding text field.
                            if (DatabaseProviderHelper.IsDatabaseProviderProvidedByEnvironment())
                            {
                                var contextId = Path.GetFileName(Path.GetDirectoryName(contentRootPath));
                                argumentsBuilder.AddWithValue("OrchardCore:TablePrefix", contextId.Replace('-', '_'));
                            }

                            return Task.CompletedTask;
                        };

                    if (changeConfigurationAsync != null) await changeConfigurationAsync(configuration);
                });
    }
}
