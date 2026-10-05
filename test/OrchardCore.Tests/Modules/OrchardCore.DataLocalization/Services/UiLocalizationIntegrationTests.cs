using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
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
        const string sourceContext = "OrchardCore.DataLocalization.Views.UiTranslations.Index";
        const string untrusted = "<img src=x onerror=alert(1)>";
        var page = await GetPageAsync(context,
            "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        var form = page.QuerySelectorAll("form").Single(element =>
            element.QuerySelector("input[name=context]")?.GetAttribute("value") == sourceContext);
        var token = form.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value");
        var fields = new Dictionary<string, string>
        {
            ["culture"] = "fr",
            ["context"] = sourceContext,
            ["key"] = "UI Translations",
            ["values"] = untrusted,
        };

        using (var rejected = await context.Client.PostAsync("Admin/Localization/UI/Save", new FormUrlEncodedContent(fields), TestContext.Current.CancellationToken))
        {
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        fields["__RequestVerificationToken"] = token;
        using (var saved = await context.Client.PostAsync("Admin/Localization/UI/Save", new FormUrlEncodedContent(fields), TestContext.Current.CancellationToken))
        {
            Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        }

        page = await GetPageAsync(context,
            "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        Assert.Equal(untrusted, page.QuerySelector("h1").TextContent);
        Assert.Null(page.QuerySelector("img[onerror]"));
        var export = await context.Client.GetStringAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        Assert.Contains("msgctxt \"" + sourceContext + "\"", export, StringComparison.Ordinal);
        Assert.Contains("msgstr \"" + untrusted + "\"", export, StringComparison.Ordinal);

        form = page.QuerySelectorAll("form").Single(element =>
            element.QuerySelector("input[name=context]")?.GetAttribute("value") == sourceContext);
        fields["__RequestVerificationToken"] = form.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value");
        fields["remove"] = "true";
        using (var removed = await context.Client.PostAsync("Admin/Localization/UI/Save", new FormUrlEncodedContent(fields), TestContext.Current.CancellationToken))
        {
            Assert.Equal(HttpStatusCode.Redirect, removed.StatusCode);
        }

        page = await GetPageAsync(context, "Admin/Localization/UI/Index?search=UI%20Translations", "fr");
        Assert.Equal("UI Translations", page.QuerySelector("h1").TextContent);
        using var upload = new MultipartFormDataContent();
        upload.Add(new StringContent("fr"), "culture");
        upload.Add(new StringContent(page.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value")), "__RequestVerificationToken");
        upload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(export)), "file", "overrides.po");
        using var imported = await context.Client.PostAsync("Admin/Localization/UI/Import", upload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, imported.StatusCode);
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
        Assert.Null(page.QuerySelector("input[type=file]"));
        Assert.All(page.QuerySelectorAll("textarea"), textarea => Assert.True(textarea.HasAttribute("disabled")));
        var fields = new Dictionary<string, string>
        {
            ["culture"] = "fr",
            ["context"] = "OrchardCore.DataLocalization.UiLocalizationAdminMenu",
            ["key"] = "UI Translations",
            ["values"] = "Unauthorized",
            ["__RequestVerificationToken"] = page.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value"),
        };
        using var response = await context.Client.PostAsync("Admin/Localization/UI/Save", new FormUrlEncodedContent(fields), TestContext.Current.CancellationToken);
        AssertAccessDenied(response);
        using var export = await context.Client.GetAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        export.EnsureSuccessStatusCode();

        using var upload = new MultipartFormDataContent();
        upload.Add(new StringContent("fr"), "culture");
        upload.Add(new StringContent(fields["__RequestVerificationToken"]), "__RequestVerificationToken");
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
        Assert.Equal("fr", Assert.Single(page.QuerySelectorAll("select[name=Culture] option")).GetAttribute("value"));
        Assert.NotNull(page.QuerySelector("input[type=file]"));

        using var ownExport = await context.Client.GetAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        ownExport.EnsureSuccessStatusCode();
        using var otherExport = await context.Client.GetAsync("Admin/Localization/UI/Export?culture=ru", TestContext.Current.CancellationToken);
        AssertAccessDenied(otherExport);
        using var otherIndex = await context.Client.GetAsync("Admin/Localization/UI/Index?culture=ru", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, otherIndex.StatusCode);

        using var fields = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["culture"] = "FR",
            ["context"] = "OrchardCore.DataLocalization.UiLocalizationAdminMenu",
            ["key"] = "UI Translations",
            ["values"] = "Authorized French override",
            ["__RequestVerificationToken"] = page.QuerySelector("input[name=__RequestVerificationToken]").GetAttribute("value"),
        });
        using var saved = await context.Client.PostAsync("Admin/Localization/UI/Save", fields, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        var export = await context.Client.GetStringAsync("Admin/Localization/UI/Export?culture=fr", TestContext.Current.CancellationToken);
        Assert.Contains("Authorized French override", export, StringComparison.Ordinal);
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
