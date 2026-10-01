using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Users' user-fields.ts (80 net-new lines): the password
// generator/copy/show-hide wiring on the "Create User" admin form
// (only rendered when Model.IsNewRequest, i.e. /Admin/Users/Create).
//
// request-verification-code.ts (31 net-new lines, shared by
// EmailAuthenticatorValidation.cshtml/SmsAuthenticatorValidation.cshtml) is NOT covered
// here: exercising it needs a real mid-login two-factor-authentication challenge (a user
// with email/SMS 2FA enabled, an in-flight login attempt stopped at the 2FA step), which
// is materially more setup than this task's scope - left as a follow-up if full Task 11
// coverage is wanted.
public sealed class UserFieldsTests : CmsTestBase<UserFieldsTestsFixture>, IClassFixture<UserFieldsTestsFixture>
{
    public UserFieldsTests(UserFieldsTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task PasswordGenerator_GeneratesFillsAndTogglesVisibility()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Users/Create");

        var passwordInput = page.Locator("input.password-input-field");
        var confirmationInput = page.Locator("input.password-confirmation-input-field");
        var generateButton = page.Locator(".password-generator-button");
        var passwordToggle = page.Locator(".password-toggle-button");
        var confirmationToggle = page.Locator(".password-confirmation-toggle-button");

        await Assertions.Expect(passwordInput).ToHaveCountAsync(1);
        await Assertions.Expect(passwordInput).ToHaveAttributeAsync("type", "password");
        Assert.Equal(string.Empty, await passwordInput.InputValueAsync());

        // Clicking "Generate password" should fill both fields with the same non-empty
        // value (and NOT submit the form - it's a plain div.btn, not a submit button).
        await generateButton.ClickAsync();

        var generatedPassword = await passwordInput.InputValueAsync();
        Assert.False(string.IsNullOrEmpty(generatedPassword), "Expected a generated password to be filled in.");
        Assert.Equal(generatedPassword, await confirmationInput.InputValueAsync());
        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Admin/Users/Create$"));

        // Toggling visibility should flip both the input's type and the icon class -
        // toggling the primary field's button affects BOTH password fields (shared
        // togglePasswordFieldState handler), per the real source.
        await passwordToggle.ClickAsync();
        await Assertions.Expect(passwordInput).ToHaveAttributeAsync("type", "text");
        await Assertions.Expect(confirmationInput).ToHaveAttributeAsync("type", "text");
        await Assertions.Expect(passwordToggle.Locator(".toggle-icon")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bfa-eye-slash\b"));

        // Toggling back via the confirmation field's own button should also flip both.
        await confirmationToggle.ClickAsync();
        await Assertions.Expect(passwordInput).ToHaveAttributeAsync("type", "password");
        await Assertions.Expect(confirmationInput).ToHaveAttributeAsync("type", "password");
        await Assertions.Expect(passwordToggle.Locator(".toggle-icon")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bfa-eye\b"));

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
