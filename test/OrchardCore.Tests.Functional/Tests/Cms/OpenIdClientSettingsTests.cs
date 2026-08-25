using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.OpenId's openid-client-settings.ts, a hand-rewritten conditional-
// visibility state machine: exactly one of the mutually-exclusive flow checkboxes/response-
// mode select may be active at a time (checking one unchecks all siblings), and the Client
// Secret field's containing Bootstrap collapse is shown only when a flow that needs a
// secret is selected (Code / Code+IdToken / Code+Token / Code+IdToken+Token - NOT the
// Implicit flows, which set useClientSecret = false server-side, matching refreshFlows()'s
// showSecret computation client-side).
//
// Writing this test caught a real, previously-unshipped regression from the Vue2->Vue3
// migration (PR #19774): OpenIdClientSettings.Edit.cshtml's script tag declared
// depends-on="parametersEditor" - but "parametersEditor" is only a CSS (Sass) resource
// (see OrchardCore.OpenId/Assets.json), never registered as a script resource. Because
// ResourceManager.ExpandDependenciesImplementation's FindResource() silently no-ops on an
// unresolved dependency name (see ResourceManager.cs), the real "vuejs"/"vue-draggable"
// script dependencies this component actually needs (the same ones every other
// options-table-editor.ts consumer, e.g. SeoMetaPart.Edit.cshtml, correctly declares) were
// simply never loaded. The module's own script then threw `ReferenceError: vuedraggable is
// not defined` on the very first statement inside initOptionsTableEditor() - before the
// change-event listeners that drive the whole flow-selection state machine were ever
// attached, silently breaking the entire page's interactivity. No console-error assertion
// in a full page-load smoke test would have caught this either, since Playwright's
// page.EvaluateAsync(dynamic import) was needed to surface the real thrown error - the
// browser's own module error reporting for a `<script type="module">` tag doesn't
// necessarily surface as a page "pageerror"/console entry in every environment. Fixed by
// correcting depends-on to "vuejs, vue-draggable", matching every sibling consumer.
public sealed class OpenIdClientSettingsTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public OpenIdClientSettingsTests(BlogFixture fixture) : base(fixture) { }

    private const string SettingsUrl = "/Admin/Settings/OrchardCore.OpenId.Client";

    private static async Task EnsureFeatureEnabledAsync(IPage page)
    {
        await page.GotoAsync("/Admin/Features");
        var enableButton = page.Locator("#btn-enable-OrchardCore_OpenId_Client");
        if (await enableButton.CountAsync() > 0)
        {
            await enableButton.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Enabling the feature with no configuration yet triggers a background
            // "OpenID client settings are invalid" warning as soon as any request resolves
            // OpenIdClientConfiguration - fill in the minimum required fields once so the
            // shared BlogFixture's AssertNoLoggedIssues() (checked once per fixture, not
            // per test) doesn't fail on unrelated log noise for every test in this class.
            // This also means every test below loads the page with Code flow ALREADY
            // checked server-side, so each test must establish its own known baseline
            // rather than assume "nothing checked yet".
            await page.GotoAndAssertOkAsync(SettingsUrl);
            await page.Locator("input#Authority, input[name$='Authority']").First.FillAsync("https://example.test/");
            await page.Locator(".openid-use-code-flow").ClickAsync();
            await page.Locator(".btn.save").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    // Every flow checkbox on this page starts server-checked from EnsureFeatureEnabledAsync
    // (Code flow, to satisfy the settings validator) - Playwright's CheckAsync() is a no-op
    // (fires no "change" event) on an already-checked box, so every test must first uncheck
    // ALL flow checkboxes to establish a real "nothing selected" baseline that a subsequent
    // click will genuinely trigger the change listener for. ClickAsync() (not CheckAsync/
    // UncheckAsync) is used throughout this file for the same reason: it's a real user
    // gesture that reliably dispatches a trusted "change" event; the Check/Uncheck locator
    // actions are no-ops when the checkbox is already in the target state.
    private static async Task ResetToNoFlowSelectedAsync(IPage page)
    {
        var allFlowCheckboxes = page.Locator(
            ".openid-use-code-flow, .openid-use-code-id-token-flow, .openid-use-code-token-flow, " +
            ".openid-use-code-id-token-token-flow, .openid-use-id-token-flow, .openid-use-id-token-token-flow");
        var count = await allFlowCheckboxes.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var checkbox = allFlowCheckboxes.Nth(i);
            if (await checkbox.IsCheckedAsync())
            {
                await checkbox.ClickAsync();
            }
        }

        await page.WaitForTimeoutAsync(400); // Bootstrap collapse transition.
    }

    [Fact]
    public async Task CheckingCodeFlow_RevealsClientSecret_AndUnchecksOtherFlows()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await EnsureFeatureEnabledAsync(page);
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync(SettingsUrl);
        await ResetToNoFlowSelectedAsync(page);

        var useCodeFlow = page.Locator(".openid-use-code-flow");
        var useIdTokenFlow = page.Locator(".openid-use-id-token-flow");
        var clientSecretContainer = page.Locator(".openid-client-secret").Locator("xpath=ancestor::div[contains(@class, 'collapse')][1]");

        // Start from a known state: the Implicit id_token flow selected, which does NOT
        // need a client secret - this is also the state whose "does the secret section
        // start hidden" assumption the second assertion below depends on.
        await useIdTokenFlow.ClickAsync();
        await page.WaitForTimeoutAsync(400); // Bootstrap collapse transition.
        await Assertions.Expect(clientSecretContainer).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));

        await useCodeFlow.ClickAsync();
        await page.WaitForTimeoutAsync(400);

        // Checking Code flow must both reveal the secret field...
        await Assertions.Expect(clientSecretContainer).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));
        // ...and uncheck the previously-selected mutually-exclusive Implicit flow - this is
        // the actual state-machine behavior refreshFlows()/the change listener implement,
        // not just an isolated show/hide toggle.
        await Assertions.Expect(useIdTokenFlow).Not.ToBeCheckedAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task UncheckingAllFlows_HidesClientSecretAgain()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await EnsureFeatureEnabledAsync(page);
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync(SettingsUrl);
        await ResetToNoFlowSelectedAsync(page);

        var useCodeFlow = page.Locator(".openid-use-code-flow");
        var clientSecretContainer = page.Locator(".openid-client-secret").Locator("xpath=ancestor::div[contains(@class, 'collapse')][1]");

        await Assertions.Expect(clientSecretContainer).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));

        await useCodeFlow.ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await Assertions.Expect(clientSecretContainer).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));

        await useCodeFlow.ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await Assertions.Expect(clientSecretContainer).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task UncheckingCodeFlow_DisablesQueryResponseMode_AndResetsToFormPost()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await EnsureFeatureEnabledAsync(page);
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync(SettingsUrl);
        await ResetToNoFlowSelectedAsync(page);

        var useCodeFlow = page.Locator(".openid-use-code-flow");
        var responseMode = page.Locator(".openid-response-mode");
        var queryOption = page.Locator(".openid-response-mode-query");

        // The "query" response mode option is only ever meaningful for the Code flow -
        // refreshFlows() disables it and forces the select back to "form_post" whenever
        // Code flow is off.
        await useCodeFlow.ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await Assertions.Expect(queryOption).Not.ToBeDisabledAsync();

        await responseMode.SelectOptionAsync(new SelectOptionValue { Label = await queryOption.TextContentAsync() });
        await useCodeFlow.ClickAsync();
        await page.WaitForTimeoutAsync(400);

        await Assertions.Expect(queryOption).ToBeDisabledAsync();

        var formPostValue = await responseMode.GetAttributeAsync("data-form-post-value");
        await Assertions.Expect(responseMode).ToHaveValueAsync(formPostValue ?? string.Empty);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
