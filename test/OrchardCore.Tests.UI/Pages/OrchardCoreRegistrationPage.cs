using System.Threading.Tasks;
using Atata;
using Lombiq.Tests.UI.Attributes.Behaviors;
using Lombiq.Tests.UI.Components;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Services;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreRegistrationPage;
#pragma warning restore IDE0065 // Misplaced using directive

    [Url(DefaultUrl)]
    [TermFindSettings(Case = TermCase.Pascal, TargetAllChildren = true, TargetAttributeType = typeof(FindByNameAttribute))]
    public class OrchardCoreRegistrationPage : Page<_>
    {
        public const string DefaultUrl = "Register";

        [FindByName]
        public TextInput<_> UserName { get; private set; }

        [FindByName]
        [SetsValueReliably]
        public TextInput<_> Email { get; private set; }

        [FindByName]
        public PasswordInput<_> Password { get; private set; }

        [FindByName]
        public PasswordInput<_> ConfirmPassword { get; private set; }

        [FindByName("RegistrationCheckbox")]
        public CheckBox<_> PrivacyPolicyAgreement { get; private set; }

        [FindByAttribute("type", "submit")]
        public Button<_> Register { get; private set; }

        public ValidationMessageList<_> ValidationMessages { get; private set; }

        public _ ShouldStayOnRegistrationPage() =>
            PageUrl.Should.StartWith(Context.BaseUrl + DefaultUrl);

        public _ ShouldLeaveRegistrationPage() =>
            PageUrl.Should.Not.StartWith(Context.BaseUrl + DefaultUrl);

        public async Task<_> RegisterWithAsync(UITestContext context, UserRegistrationParameters parameters)
        {
            UserName.Set(parameters.UserName);
            Email.Set(parameters.Email);
            Password.Set(parameters.Password);
            ConfirmPassword.Set(parameters.ConfirmPassword);
            Register.Click();

            await context.TriggerAfterPageChangeEventAndRefreshAtataContextAsync();

            context.RefreshCurrentAtataContext();

            return this;
        }
    }
}
