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

        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Create");
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
        await Assertions.Expect(entry).ToContainTextAsync("TextSecret");
        await Assertions.Expect(entry).ToContainTextAsync("Database");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("original-secret-value");

        await entry.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "TestApiKey", Exact = true }).ClickAsync();

        await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/Admin/Secrets/Edit/TestApiKey\?store=Database$"));
        await Assertions.Expect(page.Locator("#Name")).ToHaveValueAsync("TestApiKey");
        await Assertions.Expect(page.Locator("#Name")).ToHaveAttributeAsync("readonly", string.Empty);
        await Assertions.Expect(page.Locator("#Store")).ToHaveValueAsync("Database");
        await Assertions.Expect(page.Locator("#Store")).ToHaveAttributeAsync("readonly", string.Empty);
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
    public async Task StoreManagement_RequiresConfirmationAndDoesNotExposeValues()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "FirstKey", "first-private-value");
        await CreateTextSecretAsync(page, "SecondKey", "second-private-value");
        await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Manage Stores", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Active secrets: 2");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("first-private-value");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("second-private-value");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Execute", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Confirm the operation");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Active secrets: 2");
        await page.GotoAndAssertOkAsync($"{Tenant.Prefix}/Admin/Secrets/Stores/Index?sourceStore=Database");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Active secrets: 2");
        await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Back to Secrets" }).ClickAsync();
        await Assertions.Expect(SecretEntry(page, "FirstKey")).ToHaveCountAsync(1);
        await Assertions.Expect(SecretEntry(page, "SecondKey")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task MovePage_ContainsOnlyMetadataAndRejectsMissingDestination()
    {
        var page = await CreateAdminPageAsync();
        await CreateTextSecretAsync(page, "MoveKey", "move-private-value");
        await SecretEntry(page, "MoveKey").GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Move", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Move secret: MoveKey");
        await Assertions.Expect(page.Locator("body")).Not.ToContainTextAsync("move-private-value");
        await page.Locator("#Confirm").CheckAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Execute", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Select a different writable destination store");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Active secrets: 1");
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
