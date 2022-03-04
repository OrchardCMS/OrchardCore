using System;
using System.Threading.Tasks;
using Atata;
using Atata.Bootstrap;
using Lombiq.Tests.UI.Attributes.Behaviors;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreSetupPage;
#pragma warning restore IDE0065 // Misplaced using directive

    [VerifyTitle(values: new[] { DefaultPageTitle, OlderPageTitle }, Format = "{0}")]
    [VerifyH1(values: new[] { DefaultPageTitle, OlderPageTitle })]
    [TermFindSettings(
        Case = TermCase.Pascal,
        TargetAllChildren = true,
        TargetAttributeTypes = new[] { typeof(FindByIdAttribute), typeof(FindByNameAttribute) })]
    public sealed class OrchardCoreSetupPage : Page<_>
    {
        public const string DefaultPageTitle = "Setup";
        public const string OlderPageTitle = "Orchard Setup";

        public enum DatabaseType
        {
            [Term("Sql Server")]
            SqlServer,
            Sqlite,
            MySql,
            Postgres,
        }

        [FindById("culturesList")]
        [SelectsOptionByValue]
        public Select<_> Language { get; private set; }

        [FindByName]
        public TextInput<_> SiteName { get; private set; }

        [FindById("recipeButton")]
        public BSDropdownToggle<_> Recipe { get; private set; }

        [FindById]
        [SelectsOptionByValue]
        public Select<_> SiteTimeZone { get; private set; }

        [FindById]
        public Select<DatabaseType, _> DatabaseProvider { get; private set; }

        [FindById]
        public PasswordInput<_> ConnectionString { get; private set; }

        [FindById]
        public TextInput<_> TablePrefix { get; private set; }

        [FindByName]
        public TextInput<_> UserName { get; private set; }

        [FindByName]
        [SetsValueReliably]
        public EmailInput<_> Email { get; private set; }

        [FindByName]
        public PasswordInput<_> Password { get; private set; }

        [FindByName]
        public PasswordInput<_> PasswordConfirmation { get; private set; }

        public Button<_> FinishSetup { get; private set; }

        public _ ShouldStayOnSetupPage() => PageTitle.Should.Satisfy(title => IsExpectedTitle(title));

        public _ ShouldLeaveSetupPage() => PageTitle.Should.Not.Satisfy(title => IsExpectedTitle(title));

        public async Task<_> SetupOrchardCoreAsync(UITestContext context, OrchardCoreSetupParameters parameters = null)
        {
            parameters ??= new OrchardCoreSetupParameters();

            Language.Set(parameters.LanguageValue);
            SiteName.Set(parameters.SiteName);
            // If there are a lot of recipes and "headless" mode is disabled, the recipe can become unclickable because
            // the list of recipes is too long and it's off the screen, so we need to use JavaScript for clicking it.
            context
                .ExecuteScript("document.querySelectorAll(\"a[data-recipe-name='" + parameters.RecipeId + "']\")[0]" +
                ".click()");
            DatabaseProvider.Set(parameters.DatabaseProvider);

            if (!string.IsNullOrWhiteSpace(parameters.SiteTimeZoneValue))
            {
                SiteTimeZone.Set(parameters.SiteTimeZoneValue);
            }

            if (parameters.DatabaseProvider != DatabaseType.Sqlite)
            {
                if (string.IsNullOrEmpty(parameters.ConnectionString))
                {
                    throw new InvalidOperationException(
                        $"{nameof(OrchardCoreSetupParameters)}.{nameof(parameters.DatabaseProvider)}: " +
                        "If the selected database provider is other than SQLite a connection string must be provided.");
                }

                if (!string.IsNullOrEmpty(parameters.TablePrefix)) TablePrefix.Set(parameters.TablePrefix);
                ConnectionString.Set(parameters.ConnectionString);
            }

            Email.Set(parameters.Email);
            UserName.Set(parameters.UserName);
            Password.Set(parameters.Password);
            PasswordConfirmation.Set(parameters.Password);

            FinishSetup.Click();

            await context.TriggerAfterPageChangeEventAndRefreshAtataContextAsync();

            context.RefreshCurrentAtataContext();

            return this;
        }

        private static bool IsExpectedTitle(string title) =>
            title.EqualsOrdinalIgnoreCase(DefaultPageTitle) || title.EqualsOrdinalIgnoreCase(OlderPageTitle);
    }
}
