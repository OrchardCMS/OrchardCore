using System.Net.Http.Json;
using System.Text.Json;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.DataLocalization.ViewModels;
using OrchardCore.Admin;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Localization;
using OrchardCore.Tests.Modules.OrchardCore.Users;

namespace OrchardCore.DataLocalization.Services.Tests;

public sealed class UiLocalizationIntegrationTests
{
    [Fact]
    public async Task Feature_EnableUiOverrides_EnablesDynamicLocalizationAndCultures()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await features.GetAvailableFeaturesAsync()).Single(value => value.Id == "OrchardCore.DataLocalization.Ui");
            await features.EnableFeaturesAsync([feature], force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var enabled = await features.GetEnabledFeaturesAsync();
            Assert.Contains(enabled, feature => feature.Id == "OrchardCore.DataLocalization");
            Assert.Contains(enabled, feature => feature.Id == "OrchardCore.Localization");
            Assert.NotEmpty(scope.ServiceProvider.GetServices<ILocalizationDataProvider>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<TranslationsManager>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUiTranslationsManager>());
        });
    }

    [Fact]
    public async Task Runtime_CommittedUpdatesRemovalAndDisabledFeature_InvalidatesAndRestoresPo()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr"], "OrchardCore.DataLocalization.Ui");
        await TenantLocalizationTestHelper.WritePoFileAsync(context, "fr",
            ("OrchardCore.DataLocalization.UiLocalizationAdminMenu", "UI Translations", "PO translation"));

        await AssertTranslationAsync("PO translation");
        await SaveAsync(["Database translation"]);
        await AssertTranslationAsync("Database translation");
        await SaveAsync(["Updated translation"]);
        await AssertTranslationAsync("Updated translation");
        await SaveAsync([]);
        await AssertTranslationAsync("PO translation");
        await SaveAsync(["Stored but disabled"]);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await features.GetAvailableFeaturesAsync()).Single(value => value.Id == "OrchardCore.DataLocalization.Ui");
            await features.DisableFeaturesAsync([feature], force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        await AssertTranslationAsync("PO translation");

        Task AssertTranslationAsync(string expected)
            => context.UsingTenantScopeAsync(scope =>
            {
                using var culture = CultureScope.Create("fr");
                var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<UiLocalizationAdminMenu>>();
                Assert.Equal(expected, localizer["UI Translations"].Value);
                return Task.CompletedTask;
            });

        Task SaveAsync(string[] values)
            => context.UsingTenantScopeAsync(async scope =>
            {
                await scope.ServiceProvider.GetRequiredService<IUiTranslationsManager>().UpdateAsync("fr", [
                    new UiTranslation { Context = "OrchardCore.DataLocalization.UiLocalizationAdminMenu", Key = "UI Translations", Values = values },
                ]);
            });
    }

    [Fact]
    public async Task Admin_RazorSaveImportExportRemovalAndAntiforgery_WorkEndToEnd()
    {
        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = [AdminPermissions.AccessAdminPanel, DataLocalizationPermissions.ManageTranslations, DataLocalizationPermissions.ViewDynamicTranslations],
        });
        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr"], "OrchardCore.DataLocalization.Ui");
        const string sourceContext = "OrchardCore.DataLocalization.Views.Admin.Index";
        const string untrusted = "<img src=x onerror=alert(1)>";
        var page = await GetPageAsync(context,
            "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        var editor = page.QuerySelector("#translation-editor");
        Assert.Equal("true", editor.GetAttribute("data-ui-localization"));
        Assert.NotNull(editor.QuerySelector("#auto-save-toggle"));
        Assert.NotNull(editor.QuerySelector("#missing-only-toggle"));
        Assert.NotNull(editor.QuerySelector("button.save"));
        var token = editor.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value");
        var translation = new UiTranslation { Context = sourceContext, Key = "UI Translations", Values = [untrusted] };
        Assert.Contains(GetEntries(editor), entry => entry.Context == sourceContext && entry.Key == translation.Key);

        using (var rejected = await SaveAsync(context, "fr", [translation]))
        {
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        using (var saved = await SaveAsync(context, "fr", [translation], token))
        {
            Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        }

        page = await GetPageAsync(context,
            "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        Assert.Equal(untrusted, page.QuerySelector("h1").TextContent);
        Assert.Null(page.QuerySelector("img[onerror]"));
        editor = page.QuerySelector("#translation-editor");
        Assert.Equal(untrusted, GetEntries(editor).Single(entry => entry.Context == sourceContext && entry.Key == translation.Key).Values[0]);
        var export = await context.Client.GetStringAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        Assert.Contains("msgctxt \"" + sourceContext + "\"", export, StringComparison.Ordinal);
        Assert.Contains("msgstr \"" + untrusted + "\"", export, StringComparison.Ordinal);

        token = editor.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value");
        translation.Values = [];
        using (var removed = await SaveAsync(context, "fr", [translation], token))
        {
            Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
        }

        page = await GetPageAsync(context, "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        Assert.Equal("UI Translations", page.QuerySelector("h1").TextContent);
        using var upload = new MultipartFormDataContent();
        upload.Add(new StringContent("fr"), "culture");
        upload.Add(new StringContent(page.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value")), "__RequestVerificationToken");
        upload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(export)), "file", "overrides.po");
        using var imported = await context.Client.PostAsync("Admin/Localization/UI/Import", upload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, imported.StatusCode);
        page = await GetPageAsync(context, "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        Assert.Equal(untrusted, page.QuerySelector("h1").TextContent);
    }

    [Fact]
    public async Task Admin_ReadOnlyPermissions_RejectMutationsAndPreserveExportAccess()
    {
        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = [AdminPermissions.AccessAdminPanel, DataLocalizationPermissions.ViewDynamicTranslations],
        });
        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr"], "OrchardCore.DataLocalization.Ui");
        var page = await GetPageAsync(context, "Admin/Localization/UI/Index", "fr");
        Assert.Equal("true", page.QuerySelector("#translation-editor").GetAttribute("data-is-read-only"));
        var token = page.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value");
        using var response = await SaveAsync(context, "fr", [
            new UiTranslation { Context = "OrchardCore.DataLocalization.UiLocalizationAdminMenu", Key = "UI Translations", Values = ["Unauthorized"] },
        ], token);
        AssertAccessDenied(response);
        using var export = await context.Client.GetAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        export.EnsureSuccessStatusCode();

        using var upload = new MultipartFormDataContent();
        upload.Add(new StringContent("fr"), "culture");
        upload.Add(new StringContent(token), "__RequestVerificationToken");
        upload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("msgid \"Hello\"\nmsgstr \"Unauthorized\"")), "file", "overrides.po");
        using var imported = await context.Client.PostAsync("Admin/Localization/UI/Import", upload, TestContext.Current.CancellationToken);
        AssertAccessDenied(imported);
    }

    [Fact]
    public async Task Admin_CulturePermission_RestrictsPickerAndExportAndAuthorizesOwnCulture()
    {
        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = [AdminPermissions.AccessAdminPanel, DataLocalizationPermissions.CreateCulturePermission("fr", "French")],
        });
        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr", "ru"], "OrchardCore.DataLocalization.Ui");
        var page = await GetPageAsync(context, "Admin/Localization/UI/Index", "fr");
        var editor = page.QuerySelector("#translation-editor");
        var cultures = JsonSerializer.Deserialize<CultureViewModel[]>(editor.GetAttribute("data-cultures"), JsonOptions);
        Assert.Equal("fr", Assert.Single(cultures).Name);
        Assert.NotNull(page.QuerySelector("input[type=file]"));

        using var ownExport = await context.Client.GetAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        ownExport.EnsureSuccessStatusCode();
        using var otherExport = await context.Client.GetAsync("Admin/Localization/UI/Export?culture=ru", TestContext.Current.CancellationToken);
        AssertAccessDenied(otherExport);
        using var otherIndex = await context.Client.GetAsync("Admin/Localization/UI/Index?culture=ru", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, otherIndex.StatusCode);

        using var otherStrings = await context.Client.GetAsync("Admin/Localization/UI/GetStrings?culture=ru", TestContext.Current.CancellationToken);
        AssertAccessDenied(otherStrings);
        using var saved = await SaveAsync(context, "FR", [
            new UiTranslation { Context = "OrchardCore.DataLocalization.UiLocalizationAdminMenu", Key = "UI Translations", Values = ["Authorized French override"] },
        ], page.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value"));
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var export = await context.Client.GetStringAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        Assert.Contains("Authorized French override", export, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Editor_CultureSwitchAndBatchValidation_PreservesPluralsContextsAndUnchangedOverrides()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr", "ar"], "OrchardCore.DataLocalization.Ui");
        var page = await GetPageAsync(context, "Admin/Localization/UI/Index", "fr");
        var editor = page.QuerySelector("#translation-editor");
        Assert.Contains(GetEntries(editor), entry => entry.Metadata.Length > 0);
        var token = editor.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value");
        const string menuContext = "OrchardCore.DataLocalization.UiLocalizationAdminMenu";
        var first = new UiTranslation { Context = menuContext, Key = "UI Translations", Values = ["Keep this"] };
        using (var saved = await SaveAsync(context, "fr", [first], token))
        {
            saved.EnsureSuccessStatusCode();
        }

        var second = new UiTranslation { Context = "OrchardCore.DataLocalization.Views.Admin.Index", Key = "UI Translations", Values = ["Independent context"] };
        using (var saved = await SaveAsync(context, "fr", [second], token))
        {
            saved.EnsureSuccessStatusCode();
        }

        using var stringsResponse = await context.Client.GetAsync("Admin/Localization/UI/GetStrings?culture=fr", TestContext.Current.CancellationToken);
        stringsResponse.EnsureSuccessStatusCode();
        using var strings = JsonDocument.Parse(await stringsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var entries = GetEntries(strings.RootElement.GetProperty("providers").GetRawText());
        Assert.Equal("Keep this", entries.Single(entry => entry.Context == menuContext && entry.Key == first.Key).Values[0]);
        Assert.Equal("Independent context", entries.Single(entry => entry.Context == second.Context && entry.Key == second.Key).Values[0]);

        using var arabicResponse = await context.Client.GetAsync("Admin/Localization/UI/GetStrings?culture=ar", TestContext.Current.CancellationToken);
        arabicResponse.EnsureSuccessStatusCode();
        using var arabic = JsonDocument.Parse(await arabicResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var plural = GetEntries(arabic.RootElement.GetProperty("providers").GetRawText()).First(entry => entry.Plural != null);
        Assert.Equal(6, plural.Values.Length);
        first.Values = ["Should not be saved"];
        using var invalid = await SaveAsync(context, "fr", [first, new UiTranslation { Context = "Unknown", Key = "Invalid", Values = ["Invalid"] }], token);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var exported = await context.Client.GetStringAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        Assert.Contains("Keep this", exported, StringComparison.Ordinal);
        Assert.DoesNotContain("Should not be saved", exported, StringComparison.Ordinal);
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static IEnumerable<TranslatableStringViewModel> GetEntries(AngleSharp.Dom.IElement editor)
        => GetEntries(editor.GetAttribute("data-providers"));

    private static IEnumerable<TranslatableStringViewModel> GetEntries(string json)
        => JsonSerializer.Deserialize<TranslatableStringGroupViewModel[]>(json, JsonOptions)
            .SelectMany(group => group.Strings.Concat(group.SubGroups.SelectMany(subGroup => subGroup.Strings)));

    private static async Task<HttpResponseMessage> SaveAsync(SiteContext context, string culture, UiTranslation[] translations, string token = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "Admin/Localization/UI/Save")
        {
            Content = JsonContent.Create(new UiTranslationUpdateModel { Culture = culture, Translations = translations }),
        };
        if (token != null)
        {
            request.Headers.Add("RequestVerificationToken", token);
        }

        return await context.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static void AssertAccessDenied(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Error/403", response.Headers.Location.ToString(), StringComparison.Ordinal);
    }

    private static async Task<IHtmlDocument> GetPageAsync(SiteContext context, string path, string culture)
    {
        var separator = path.Contains('?') ? '&' : '?';
        using var response = await context.Client.GetAsync($"{path}{separator}culture={culture}&ui-culture={culture}", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var cookies = CookiesHelper.ExtractCookies(response);
        if (cookies.Count > 0)
        {
            context.Client.DefaultRequestHeaders.Remove("Cookie");
            context.Client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies.Select(cookie => cookie.Key + "=" + cookie.Value)));
        }

        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
