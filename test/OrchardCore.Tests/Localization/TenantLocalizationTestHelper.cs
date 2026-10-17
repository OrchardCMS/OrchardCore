using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Localization;

/// <summary>
/// Configures the cultures and the PO files of a test tenant, and renders its pages in a given culture.
/// </summary>
public static class TenantLocalizationTestHelper
{
    /// <summary>
    /// Sets the supported cultures of the tenant, and enables the <c>OrchardCore.Localization</c> feature and the other features.
    /// </summary>
    public static async Task EnableLocalizationAsync(SiteContext context, string[] supportedCultures, params string[] featureIds)
    {
        // The supported cultures are read when the tenant pipeline is built, so they are set before the feature is enabled.
        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new LocalizationSettings
            {
                DefaultCulture = supportedCultures[0],
                SupportedCultures = supportedCultures,
            });
            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var ids = featureIds.Append("OrchardCore.Localization").ToArray();
            var features = (await featuresManager.GetAvailableFeaturesAsync()).Where(feature => ids.Contains(feature.Id)).ToArray();
            Assert.Equal(ids.Length, features.Length);
            await featuresManager.EnableFeaturesAsync(features, force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Writes a PO file for a culture in the folder of the tenant, <c>App_Data/Sites/{tenant}/Localization/{culture}.po</c>.
    /// </summary>
    /// <remarks>
    /// The translations of a culture are loaded on the first request in that culture, so write the file before that request.
    /// </remarks>
    public static Task WritePoFileAsync(SiteContext context, string culture, params (string Context, string Text, string Translation)[] entries)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var shellOptions = scope.ServiceProvider.GetRequiredService<IOptions<ShellOptions>>().Value;
            var folder = Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, context.TenantName, "Localization");
            Directory.CreateDirectory(folder);

            var content = new StringBuilder();
            foreach (var (entryContext, text, translation) in entries)
            {
                content.AppendLine($"msgctxt \"{entryContext}\"");
                content.AppendLine($"msgid \"{text}\"");
                content.AppendLine($"msgstr \"{translation}\"");
                content.AppendLine();
            }

            await File.WriteAllTextAsync(Path.Combine(folder, culture + ".po"), content.ToString(), TestContext.Current.CancellationToken);
        });

    /// <summary>
    /// Renders a page of the tenant in a culture.
    /// </summary>
    public static async Task<IHtmlDocument> GetPageAsync(SiteContext context, string path, string culture)
    {
        var separator = path.Contains('?') ? '&' : '?';
        using var response = await context.Client.GetAsync($"{path}{separator}culture={culture}&ui-culture={culture}", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
