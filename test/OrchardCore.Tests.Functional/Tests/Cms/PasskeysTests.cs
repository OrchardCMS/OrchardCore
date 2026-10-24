using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the Passkeys section of the Security page and OrchardCore.Users' passkeys.ts, which runs the WebAuthn
// ceremonies in the browser and must serialize the credentials in the shape the ASP.NET Core Identity passkey handler
// expects. A virtual authenticator of Chromium answers the prompts, as the fingerprint or face check of a device would.
//
// WebAuthn doesn't accept an IP address as the domain of a passkey, so the test browses the server with 'localhost'
// rather than the '127.0.0.1' address it listens on.
public sealed class PasskeysTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public PasskeysTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Passkey_AddedFromPasskeysPage_LogsIn()
    {
        var page = await Fixture.CreatePageAsync();
        var consoleErrors = page.CollectConsoleErrors();
        var baseUrl = Fixture.BaseUrl.Replace("127.0.0.1", "localhost", StringComparison.Ordinal).TrimEnd('/');

        var session = await page.Context.NewCDPSessionAsync(page);
        await session.SendAsync("WebAuthn.enable");
        await session.SendAsync("WebAuthn.addVirtualAuthenticator", new Dictionary<string, object>
        {
            ["options"] = new Dictionary<string, object>
            {
                ["protocol"] = "ctap2",
                ["transport"] = "internal",
                ["hasResidentKey"] = true,
                ["hasUserVerification"] = true,
                ["isUserVerified"] = true,
                ["automaticPresenceSimulation"] = true,
            },
        });

        await page.LoginAsync(baseUrl);
        await page.EnableFeatureAsync(baseUrl, "OrchardCore.Users.Passkeys");

        await page.GotoAndAssertOkAsync($"{baseUrl}/Security");
        await page.Locator("#PasskeyName").FillAsync("Virtual authenticator");
        await page.Locator("[data-passkey-register] [data-passkey-button]").ClickAsync();

        var passkey = page.Locator(".list-group-item", new PageLocatorOptions { HasText = "Virtual authenticator" });
        await Assertions.Expect(passkey).ToBeVisibleAsync();
        await Assertions.Expect(passkey).ToContainTextAsync("Never used");

        // Log off by dropping the cookies. The virtual authenticator answers the autofill request of the login page
        // on its own, as when the user picks the passkey in the autofill of the username field.
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{baseUrl}/login");
        await page.WaitForURLAsync(url => !url.Contains("/login", StringComparison.OrdinalIgnoreCase));

        await page.GotoAndAssertOkAsync($"{baseUrl}/Security");
        await Assertions.Expect(passkey).ToContainTextAsync("Last used");

        // Log off again, and log in with the button this time, in a browser that doesn't offer passkeys in the autofill.
        await page.AddInitScriptAsync("PublicKeyCredential.isConditionalMediationAvailable = () => Promise.resolve(false);");
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{baseUrl}/login");

        var passkeyButton = page.Locator("[data-passkey-login] [data-passkey-button]");
        await Assertions.Expect(passkeyButton).ToBeVisibleAsync();
        await passkeyButton.ClickAsync();

        await page.WaitForURLAsync(url => !url.Contains("/login", StringComparison.OrdinalIgnoreCase));
        await page.GotoAndAssertOkAsync($"{baseUrl}/Security");
        await Assertions.Expect(passkey).ToBeVisibleAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
