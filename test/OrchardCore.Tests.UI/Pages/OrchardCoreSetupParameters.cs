using Lombiq.Tests.UI.Constants;
using Lombiq.Tests.UI.Services;

namespace OrchardCore.Tests.UI.Pages
{
    public class OrchardCoreSetupParameters
    {
        public string LanguageValue { get; set; } = "en";
        public string SiteName { get; set; } = "Test Site";
        public string RecipeId { get; set; } = "SaaS";
        public string SiteTimeZoneValue { get; set; }
        public OrchardCoreSetupPage.DatabaseType DatabaseProvider { get; set; } = OrchardCoreSetupPage.DatabaseType.Sqlite;
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string UserName { get; set; } = DefaultUser.UserName;
        public string Email { get; set; } = DefaultUser.Email;
        public string Password { get; set; } = DefaultUser.Password;

        public OrchardCoreSetupParameters()
        {
        }

        public OrchardCoreSetupParameters(UITestContext context)
        {
            DatabaseProvider = context.Configuration.UseSqlServer
                ? OrchardCoreSetupPage.DatabaseType.SqlServer
                : OrchardCoreSetupPage.DatabaseType.Sqlite;

            ConnectionString = context.Configuration.UseSqlServer
                ? context.SqlServerRunningContext.ConnectionString
                : null;
        }
    }
}
