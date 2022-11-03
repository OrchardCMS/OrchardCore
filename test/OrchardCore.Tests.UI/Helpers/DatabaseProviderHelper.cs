using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Tests.UI.Helpers
{
    internal static class DatabaseProviderHelper
    {
        public static OrchardCoreSetupPage.DatabaseType GetCIDatabaseProvider()
        {
            return TestConfigurationManager.RootConfiguration
                .GetSection("OrchardCore")
                .GetValue("UITestingCIDatabaseProvider", OrchardCoreSetupPage.DatabaseType.Sqlite);
        }
    }
}
