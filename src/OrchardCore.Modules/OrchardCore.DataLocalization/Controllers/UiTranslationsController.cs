using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.DataLocalization.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.DataLocalization.Controllers;

[Admin("Localization/UI/{action}", "UiTranslations{action}")]
[Feature("OrchardCore.DataLocalization.Ui")]
public sealed class UiTranslationsController : Controller
{
    private readonly IAuthorizationService _authorization;
    private readonly ILocalizationService _localization;
    private readonly IUiLocalizationCatalog _catalog;
    private readonly IUiTranslationsManager _manager;
    private readonly ISiteService _site;
    private readonly IShapeFactory _shapes;
    private readonly IStringLocalizer S;

    public UiTranslationsController(IAuthorizationService authorization, ILocalizationService localization,
        IUiLocalizationCatalog catalog, IUiTranslationsManager manager, ISiteService site, IShapeFactory shapes,
        IStringLocalizer<UiTranslationsController> localizer)
    {
        _authorization = authorization;
        _localization = localization;
        _catalog = catalog;
        _manager = manager;
        _site = site;
        _shapes = shapes;
        S = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string culture, string search, string assemblyName, string status, PagerParameters pagerParameters)
    {
        if ((await GetAllowedCulturesAsync()).Length == 0)
        {
            return Forbid();
        }

        if (culture != null && !(await GetAllowedCulturesAsync()).Contains(culture, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(S["Select an authorized configured culture."].Value);
        }

        return View(await BuildModelAsync(culture, search, assemblyName, status, pagerParameters));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string culture, string context, string key, string plural, string[] values, bool remove)
    {
        if (!await CanEditAsync(culture))
        {
            return Forbid();
        }

        try
        {
            await _manager.UpdateAsync(culture, [new UiTranslation { Context = context ?? "", Key = key, Plural = plural, Values = remove ? [] : values }]);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            ModelState.AddModelError("", S["Invalid translation: {0}", exception.Message]);
            return View("Index", await BuildModelAsync(culture, key, null, null, new PagerParameters()));
        }

        return RedirectToAction(nameof(Index), new { culture, search = key });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(4 * 1024 * 1024)]
    public async Task<IActionResult> Import(string culture, IFormFile file)
    {
        if (!await CanEditAsync(culture))
        {
            return Forbid();
        }

        if (file == null || file.Length == 0 || file.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError("", S["Select a PO file no larger than 2 MB."]);
        }
        else
        {
            try
            {
                using var reader = new StreamReader(file.OpenReadStream(), new UTF8Encoding(false, true));
                await _manager.ImportAsync(culture, reader);
            }
            catch (Exception exception) when (exception is ArgumentException or FormatException)
            {
                ModelState.AddModelError("", S["Invalid PO import: {0}", exception.Message]);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Index", await BuildModelAsync(culture, null, null, null, new PagerParameters()));
        }

        return RedirectToAction(nameof(Index), new { culture });
    }

    [HttpGet]
    public async Task<IActionResult> Export(string culture)
    {
        if (!(await GetAllowedCulturesAsync()).Contains(culture, StringComparer.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (!await IsSupportedAsync(culture))
        {
            return BadRequest(S["Select a configured supported culture."].Value);
        }

        return File(Encoding.UTF8.GetBytes(await _manager.ExportAsync(culture)), "text/plain; charset=utf-8", culture + ".overrides.po");
    }

    private async Task<UiTranslationsViewModel> BuildModelAsync(string culture, string search, string assemblyName, string status, PagerParameters pagerParameters)
    {
        var cultures = await GetAllowedCulturesAsync();
        culture = cultures.FirstOrDefault(value => string.Equals(value, culture, StringComparison.OrdinalIgnoreCase)) ?? cultures.FirstOrDefault();
        var all = _catalog.GetResources();
        var document = await _manager.GetAsync();
        var overrides = culture == null ? [] : document.Translations.GetValueOrDefault(culture) ?? [];
        var overriddenKeys = overrides.Select(translation => (translation.Context, translation.Key)).ToHashSet();
        var translatedValues = overrides.ToDictionary(translation => (translation.Context, translation.Key), translation => translation.Values);
        var resources = all.Where(resource =>
            (string.IsNullOrEmpty(assemblyName) || resource.AssemblyName == assemblyName) &&
            (string.IsNullOrWhiteSpace(search) || resource.Key.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                resource.Context.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                resource.Metadata.Any(metadata => metadata.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                translatedValues.TryGetValue((resource.Context, resource.Key), out var values) &&
                    values.Any(value => value.Contains(search, StringComparison.OrdinalIgnoreCase))) &&
            (status != "overridden" || overriddenKeys.Contains((resource.Context, resource.Key))) &&
            (status != "untranslated" || !overriddenKeys.Contains((resource.Context, resource.Key))))
            .ToArray();
        var pager = new Pager(pagerParameters, (await _site.GetSiteSettingsAsync()).PageSize);
        var routeData = new RouteData();
        routeData.Values["culture"] = culture;
        routeData.Values["search"] = search;
        routeData.Values["assemblyName"] = assemblyName;
        routeData.Values["status"] = status;
        var pagerShape = await _shapes.PagerAsync(pager, resources.Length, routeData);
        return new UiTranslationsViewModel
        {
            Culture = culture,
            Cultures = cultures,
            Search = search,
            AssemblyName = assemblyName,
            Status = status,
            Assemblies = all.Select(resource => resource.AssemblyName).Distinct().ToArray(),
            Resources = resources.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToArray(),
            Overrides = overrides,
            PluralFormCount = culture == null ? 0 : _manager.GetPluralFormCount(culture),
            CanEdit = await CanEditAsync(culture),
            Pager = pagerShape,
        };
    }

    private async Task<string[]> GetAllowedCulturesAsync()
    {
        var cultures = await _localization.GetSupportedCulturesAsync();
        if (await _authorization.AuthorizeAsync(User, DataLocalizationPermissions.ViewDynamicTranslations))
        {
            return cultures;
        }

        var allowed = new List<string>();
        foreach (var culture in cultures)
        {
            if (await CanEditAsync(culture))
            {
                allowed.Add(culture);
            }
        }

        return allowed.ToArray();
    }

    private async Task<bool> IsSupportedAsync(string culture)
        => culture != null && (await _localization.GetSupportedCulturesAsync()).Contains(culture, StringComparer.OrdinalIgnoreCase);

    private async Task<bool> CanEditAsync(string culture)
    {
        culture = (await _localization.GetSupportedCulturesAsync())
            .FirstOrDefault(value => string.Equals(value, culture, StringComparison.OrdinalIgnoreCase));
        return culture != null &&
            (await _authorization.AuthorizeAsync(User, DataLocalizationPermissions.ManageTranslations) ||
             await _authorization.AuthorizeAsync(User, DataLocalizationPermissions.CreateCulturePermission(culture, CultureInfo.GetCultureInfo(culture).DisplayName)));
    }
}
