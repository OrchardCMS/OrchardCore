using System.Threading.Tasks;
using Atata;
using Lombiq.Tests.UI.Components;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreLoginPage;
#pragma warning restore IDE0065 // Misplaced using directive

    [Url(DefaultUrl)]
    [TermFindSettings(Case = TermCase.Pascal, TargetAllChildren = true, TargetAttributeType = typeof(FindByIdAttribute))]
    public class OrchardCoreLoginPage : Page<_>
    {
        private const string DefaultUrl = "Login";

        [FindById]
        public TextInput<_> UserName { get; private set; }

        [FindById]
        public PasswordInput<_> Password { get; private set; }

        [FindByAttribute("type", "submit")]
        public Button<_> LogIn { get; private set; }

        [FindByAttribute("href", TermMatch.StartsWith, "/" + OrchardCoreRegistrationPage.DefaultUrl)]
        public Link<OrchardCoreRegistrationPage, _> RegisterAsNewUser { get; private set; }

        public ValidationSummaryErrorList<_> ValidationSummaryErrors { get; private set; }

        public _ ShouldStayOnLoginPage() =>
            PageUrl.Should.StartWith(Context.BaseUrl + DefaultUrl);

        public _ ShouldLeaveLoginPage() =>
            PageUrl.Should.Not.StartWith(Context.BaseUrl + DefaultUrl);

        public async Task<_> LogInWithAsync(UITestContext context, string userName, string password)
        {
            var page = UserName.Set(userName)
                .Password.Set(password)
                .LogIn.Click();

            await context.TriggerAfterPageChangeEventAndRefreshAtataContextAsync();

            context.RefreshCurrentAtataContext();

            return page;
        }
    }
}
