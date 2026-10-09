using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class SecretsTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    private static readonly Regex s_indexUrl = new(@"/Admin/Secrets/Index(?:\?|$)");

    private IPage _page;
    private List<string> _consoleErrors = [];

    protected override string RecipeName => "Blank";

    public SecretsTests(CmsSetupFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task EnablingFeature_AddsAdminNavigationAndEmptyList()
    {
        var page = await CreateAdminPageAsync();

        await Assertions.Expect(page.Locator("#adminMenu")).ToContainTextAsync("Secrets");
        await Assertions.Expect(page.Locator(".alert-info")).ToContainTextAsync("Nothing here");
        await Assertions.Expect(page.Locator("button.create")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ExpiringSecrets_ShowAdminWideWarningAndReviewLink()
    {
        var page = await CreateAdminPageAsync();
        await Assertions.Expect(page.Locator(".secrets-expiration-warning")).ToHaveCountAsync(0);
        await CreateTextSecretAsync(page, "ExpiredKey", "expired-value",
            expiration: DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        await CreateTextSecretAsync(page, "ExpiringKey", "expiring-value",
            expiration: DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Features");

        var warning = page.Locator(".secrets-expiration-warning");
        await Assertions.Expect(warning).ToHaveCountAsync(1);
        await Assertions.Expect(warning).ToContainTextAsync("One secret has expired.");
        await Assertions.Expect(warning).ToContainTextAsync("One secret expires within 30 days.");
        await Assertions.Expect(warning).Not.ToContainTextAsync("expired-value");
        await Assertions.Expect(warning).Not.ToContainTextAsync("expiring-value");
        await warning.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Review secrets" }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(s_indexUrl);

        // The Secrets list shows the warning under its title, without the link to itself.
        warning = page.Locator(".secrets-expiration-warning");
        await Assertions.Expect(warning).ToHaveCountAsync(1);
        await Assertions.Expect(warning.GetByRole(AriaRole.Link)).ToHaveCountAsync(0);
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Stores/Index");
        await Assertions.Expect(page.Locator(".secrets-expiration-warning")).ToHaveCountAsync(0);

        await OpenEditEditorAsync(page, "ExpiredKey");
        await page.Locator("#ExpiresUtc").FillAsync(string.Empty);
        await SaveAsync(page, "updated");
        await OpenEditEditorAsync(page, "ExpiringKey");
        await page.Locator("#ExpiresUtc").FillAsync(string.Empty);
        await SaveAsync(page, "updated");
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Features");

        await Assertions.Expect(page.Locator(".secrets-expiration-warning")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task TypeSelectionModal_OpensTextEditorAndCancelReturnsToList()
    {
        var page = await CreateAdminPageAsync();
        await OpenCreateEditorAsync(page);

        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("New Text Secret");
        await Assertions.Expect(page.Locator("#SecretType")).ToHaveValueAsync("TextSecret");
        await Assertions.Expect(page.Locator("#Store")).ToHaveValueAsync("Database");

        await page.ClickCancelAsync();

        await Assertions.Expect(page).ToHaveURLAsync(s_indexUrl);
        await Assertions.Expect(page.Locator(".alert-info")).ToContainTextAsync("Nothing here");
    }

    [Fact]
    public async Task CreatingAndEditingTextSecret_PreservesIdentityAndHidesValue()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "TestApiKey", "original-secret-value", "API key description", "2030-01-01");

        var entry = SecretEntry(page, "TestApiKey");
        await Assertions.Expect(entry.Locator(".badge").Filter(new LocatorFilterOptions { HasText = "Text Secret" })).ToHaveCountAsync(1);
        await Assertions.Expect(entry.Locator(".badge").Filter(new LocatorFilterOptions { HasText = "Database" })).ToHaveCountAsync(1);
        await Assertions.Expect(entry).ToContainTextAsync("API key description");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("original-secret-value");

        await entry.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "TestApiKey", Exact = true }).ClickAsync();

        await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/Admin/Secrets/Edit/TestApiKey\?store=Database$"));
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Edit 'TestApiKey' secret");
        await Assertions.Expect(page.Locator("#Name")).ToHaveValueAsync("TestApiKey");
        await Assertions.Expect(page.Locator("#Name")).ToHaveAttributeAsync("type", "hidden");
        await Assertions.Expect(page.Locator("#Store")).ToHaveValueAsync("Database");
        await Assertions.Expect(page.Locator("#Store")).ToHaveAttributeAsync("type", "hidden");
        await Assertions.Expect(page.Locator("#TextValue")).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(page.Locator("#Description")).ToHaveValueAsync("API key description");
        await Assertions.Expect(page.Locator("#ExpiresUtc")).ToHaveValueAsync("2030-01-01");

        await page.Locator("#TextValue").FillAsync("updated-secret-value");
        await SaveAsync(page, "updated");
        await OpenEditEditorAsync(page, "TestApiKey");

        await Assertions.Expect(page.Locator("#TextValue")).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("updated-secret-value");
    }

    [Fact]
    public async Task DeletingSecret_RemovesOnlySelectedEntry()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "TestApiKey", "api-key");
        await CreateTextSecretAsync(page, "DatabasePassword", "database-password");

        await Assertions.Expect(page.Locator(".list-group-item")).ToHaveCountAsync(3);
        var entry = SecretEntry(page, "DatabasePassword");
        var delete = entry.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Delete", Exact = true });
        await Assertions.Expect(delete).ToHaveAttributeAsync("href", new Regex(@"store=Database"));
        await delete.ClickAsync();
        await page.ClickModalOkAsync();

        await Assertions.Expect(page.Locator(".message-success")).ToContainTextAsync("deleted");
        await Assertions.Expect(SecretEntry(page, "DatabasePassword")).ToHaveCountAsync(0);
        await Assertions.Expect(SecretEntry(page, "TestApiKey")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task StoreManagement_ShowsActiveCountWithoutValues()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "FirstKey", "first-private-value");
        await CreateTextSecretAsync(page, "SecondKey", "second-private-value");
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Stores/Index");

        await Assertions.Expect(page.Locator("label[for='source-store']")).ToHaveTextAsync("Source store");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Total active secrets in the selected store: 2");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Move all active secrets from this store to");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("There is no other writable store");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("first-private-value");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("second-private-value");
    }

    [Fact]
    public async Task StoreManagement_HidesMoveSectionForEmptyStore()
    {
        var page = await CreateAdminPageAsync();
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Stores/Index");

        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Total active secrets in the selected store: 0");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("Move all active secrets from this store to");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("This tenant has no active secrets");
    }

    [Fact]
    public async Task MovePage_ContainsOnlyMetadata()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "MoveKey", "move-private-value");
        await SecretEntry(page, "MoveKey").GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Move", Exact = true }).ClickAsync();

        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Move 'MoveKey' secret");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("move-private-value");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("There is no other writable store");
    }

    [Fact]
    public async Task SecretsList_FiltersBySearch()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "SearchAlpha", "alpha-value");
        await CreateTextSecretAsync(page, "SearchBeta", "beta-value");

        await page.Locator("#search-box").FillAsync("Alpha");
        await page.Locator("#search-box").PressAsync("Enter");

        await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"Options\.Search=Alpha"));
        await Assertions.Expect(SecretEntry(page, "SearchAlpha")).ToHaveCountAsync(1);
        await Assertions.Expect(SecretEntry(page, "SearchBeta")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task MigrationPage_WithoutStoredCredentials_ShowsNothingToMigrate()
    {
        var page = await CreateAdminPageAsync();
        await Assertions.Expect(page.Locator("#adminMenu a[href$='/Admin/Secrets/Migration/Index']")).ToHaveCountAsync(1);
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Migration/Index");

        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Migrate Secrets");
        await Assertions.Expect(page.Locator(".alert-info")).ToContainTextAsync("Nothing to migrate");
    }

    [Fact]
    public async Task SecretInput_ReloadsSecretsCreatedInAnotherTab()
    {
        var page = await CreateAdminPageAsync();
        await page.EnableFeatureAsync(Tenant.Prefix, "OrchardCore.Email.Smtp");
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Settings/email");
        await page.Locator(".nav-tabs a.nav-link", new PageLocatorOptions { HasText = "SMTP" }).ClickAsync();

        // The credentials are only shown once SMTP is enabled and requires credentials.
        await page.Locator("#ISite_SmtpSettings_IsEnabled").CheckAsync();
        await page.Locator("#ISite_SmtpSettings_RequireCredentials").CheckAsync();

        var source = page.Locator("#ISite_SmtpSettings_Password");
        await Assertions.Expect(source).ToHaveValueAsync("Value");
        await source.SelectOptionAsync("Secret");

        var newSecret = page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "New secret" });
        await Assertions.Expect(newSecret).ToHaveAttributeAsync("target", "_blank");
        await Assertions.Expect(newSecret).ToHaveAttributeAsync("href", new Regex(@"/Admin/Secrets/Create/Smtp\.Password\?type=TextSecret$"));

        // Create the suggested secret as if it was in the new tab.
        var tab = await page.Context.NewPageAsync();
        await tab.GotoAndAssertOkAsync(await newSecret.GetAttributeAsync("href"));
        await Assertions.Expect(tab.Locator("#Name")).ToHaveValueAsync("Smtp.Password");
        await tab.Locator("#TextValue").FillAsync("smtp-password");
        await SaveAsync(tab, "created");
        await tab.CloseAsync();

        var secrets = page.Locator("#ISite_SmtpSettings_Password_SecretName");
        await Assertions.Expect(secrets.Locator("option[value='Smtp.Password']")).ToHaveCountAsync(0);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reload the secrets" }).ClickAsync();

        await Assertions.Expect(secrets).ToHaveValueAsync("Smtp.Password");
    }

    [Fact]
    public async Task CreatingDuplicateName_ShowsValidationError()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "TestApiKey", "original-value");
        await OpenCreateEditorAsync(page);
        await page.Locator("#Name").FillAsync("TestApiKey");
        await page.Locator("#TextValue").FillAsync("duplicate-value");
        await page.ClickSaveAsync();

        await Assertions.Expect(page.Locator("[data-valmsg-for='Name']")).ToContainTextAsync("already exists");

        await page.ClickCancelAsync();
        await Assertions.Expect(SecretEntry(page, "TestApiKey")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task CreatingWithoutName_ShowsValidationError()
    {
        var page = await CreateAdminPageAsync();
        await OpenCreateEditorAsync(page);
        await page.Locator("#TextValue").FillAsync("value-without-name");
        await page.ClickSaveAsync();

        await Assertions.Expect(page.Locator("[data-valmsg-for='Name']")).ToContainTextAsync("required");
    }

    [Fact]
    public async Task EditingSecret_ClearsDescriptionAndExpiration()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "TestApiKey", "retained-value", "Description to remove", "2030-01-01");
        await OpenEditEditorAsync(page, "TestApiKey");

        await page.Locator("#Description").FillAsync(string.Empty);
        await page.Locator("#ExpiresUtc").FillAsync(string.Empty);
        await Assertions.Expect(page.Locator("#TextValue")).ToHaveValueAsync(string.Empty);
        await SaveAsync(page, "updated");
        await OpenEditEditorAsync(page, "TestApiKey");

        await Assertions.Expect(page.Locator("#Description")).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(page.Locator("#ExpiresUtc")).ToHaveValueAsync(string.Empty);
    }

    [Fact]
    public async Task EditingSecret_RejectsStoreTampering()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "TestApiKey", "original-value", "Original description");
        await OpenEditEditorAsync(page, "TestApiKey");
        await page.Locator("#Store").EvaluateAsync("element => element.value = 'AzureKeyVault'");
        await page.Locator("#Description").FillAsync("Tampered description");

        var response = await page.RunAndWaitForResponseAsync(
            () => page.ClickSaveAsync(),
            response => response.Request.Method == "POST" && response.Url.Contains("/Admin/Secrets/Edit/"));

        Assert.Equal(400, response.Status);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("400", Assert.Single(_consoleErrors));
        _consoleErrors.Clear();
        await OpenEditEditorAsync(page, "TestApiKey");
        await Assertions.Expect(page.Locator("#Store")).ToHaveValueAsync("Database");
        await Assertions.Expect(page.Locator("#Description")).ToHaveValueAsync("Original description");
    }

    [Theory]
    [InlineData("RsaKeySecret", "#RsaKeySize")]
    [InlineData("X509Secret", "#X509Thumbprint")]
    public async Task TypeSelectionModal_OpensTypeSpecificEditor(string type, string field)
    {
        var page = await CreateAdminPageAsync();
        await OpenCreateEditorAsync(page, type);

        await Assertions.Expect(page.Locator("#SecretType")).ToHaveValueAsync(type);
        await Assertions.Expect(page.Locator(field)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#TextValue")).ToHaveCountAsync(0);
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (_page != null)
            {
                await _page.CloseAsync();
            }

            Assert.Empty(_consoleErrors);
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    private async Task<IPage> CreateAdminPageAsync()
    {
        _page = await Fixture.CreatePageAsync();
        await _page.LoginAsync(Tenant.Prefix);
        _consoleErrors = _page.CollectConsoleErrors();
        await _page.EnableFeatureAsync(Tenant.Prefix, "OrchardCore.Secrets");
        await _page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Index");

        return _page;
    }

    private async Task OpenCreateEditorAsync(IPage page, string type = "TextSecret")
    {
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Index");
        await page.ClickCreateAsync();
        await Assertions.Expect(page.Locator("#modalSecretTypes")).ToBeVisibleAsync();
        await page.Locator($"#modalSecretTypes a[href*='type={type}']").ClickAsync();
        await Assertions.Expect(page.Locator("#SecretType")).ToHaveValueAsync(type);
    }

    private async Task CreateTextSecretAsync(IPage page, string name, string value, string description = null, string expiration = null)
    {
        await OpenCreateEditorAsync(page);
        await page.Locator("#Name").FillAsync(name);
        await page.Locator("#TextValue").FillAsync(value);
        if (description != null)
        {
            await page.Locator("#Description").FillAsync(description);
        }

        if (expiration != null)
        {
            await page.Locator("#ExpiresUtc").FillAsync(expiration);
        }

        await SaveAsync(page, "created");
    }

    private Task OpenEditEditorAsync(IPage page, string name) =>
        page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Edit/{name}?store=Database");

    private static async Task SaveAsync(IPage page, string message)
    {
        await page.ClickSaveAsync();
        await Assertions.Expect(page).ToHaveURLAsync(s_indexUrl);
        await Assertions.Expect(page.Locator(".message-success")).ToContainTextAsync(message);
    }

    private static ILocator SecretEntry(IPage page, string name) =>
        page.Locator(".list-group-item").Filter(new LocatorFilterOptions { HasText = name });
}
