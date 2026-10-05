using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DataLocalization.Services;
using OrchardCore.DataLocalization.ViewModels;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;

namespace OrchardCore.DataLocalization.Controllers;

[Admin("Localization/UI/{action}", "UiTranslations{action}")]
[Feature("OrchardCore.DataLocalization.Ui")]
public sealed class UiTranslationsController : Controller
{
    private readonly IAuthorizationService _authorization;
    private readonly ILocalizationService _localization;
    private readonly IUiLocalizationCatalog _catalog;
    private readonly IUiTranslationsManager _manager;
    private readonly IStringLocalizer S;

    public UiTranslationsController(IAuthorizationService authorization, ILocalizationService localization,
        IUiLocalizationCatalog catalog, IUiTranslationsManager manager,
        IStringLocalizer<UiTranslationsController> localizer)
    {
        _authorization = authorization;
        _localization = localization;
        _catalog = catalog;
        _manager = manager;
        S = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string culture, string search)
    {
        if ((await GetAllowedCulturesAsync()).Length == 0)
        {
            return Forbid();
        }

        if (culture != null && !(await GetAllowedCulturesAsync()).Contains(culture, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(S["Select an authorized configured culture."].Value);
        }

        return View(await BuildModelAsync(culture, search));
    }

    [HttpGet]
    public async Task<IActionResult> GetStrings(string culture)
    {
        if (!(await GetAllowedCulturesAsync()).Contains(culture, StringComparer.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var model = await BuildModelAsync(culture);
        return Ok(new { culture = model.CurrentCulture, providers = model.Providers });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] UiTranslationUpdateModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Culture) || model.Translations == null || !ModelState.IsValid)
        {
            return BadRequest(new { message = S["Invalid translation request."].Value });
        }

        if (!await CanEditAsync(model.Culture))
        {
            return Forbid();
        }

        try
        {
            await _manager.UpdateAsync(model.Culture, model.Translations);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            return BadRequest(new { message = S["Invalid translation: {0}", exception.Message].Value });
        }

        return Ok(new { success = true, message = S["Translations saved successfully."].Value });
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
                return BadRequest(new { message = S["Invalid PO import: {0}", exception.Message].Value });
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = S["Select a PO file no larger than 2 MB."].Value });
        }

        return Ok(new { success = true, message = S["Translations imported successfully."].Value });
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

    private async Task<TranslationEditorViewModel> BuildModelAsync(string culture, string search = null)
    {
        var cultures = await GetAllowedCulturesAsync();
        culture = cultures.FirstOrDefault(value => string.Equals(value, culture, StringComparison.OrdinalIgnoreCase)) ?? cultures.FirstOrDefault();
        var all = _catalog.GetResources();
        var document = await _manager.GetAsync();
        var overrides = culture == null ? [] : document.Translations.GetValueOrDefault(culture) ?? [];
        var translatedValues = overrides.ToDictionary(translation => (translation.Context, translation.Key), translation => translation.Values);
        var allowedCultures = new List<CultureViewModel>();
        foreach (var name in cultures)
        {
            allowedCultures.Add(new CultureViewModel
            {
                Name = name,
                DisplayName = CultureInfo.GetCultureInfo(name).DisplayName,
                CanEdit = await CanEditAsync(name),
            });
        }

        var formCount = culture == null ? 0 : _manager.GetPluralFormCount(culture);
        // A context/key can be extracted by several assemblies; edit it once and retain all metadata.
        var resources = all.GroupBy(resource => (resource.Context, resource.Key)).Select(group => new
        {
            Resource = group.First(),
            Metadata = group.SelectMany(resource => resource.Metadata).Distinct().ToArray(),
        });
        return new TranslationEditorViewModel
        {
            IsUiLocalization = true,
            CurrentCulture = culture,
            AllowedCultures = allowedCultures,
            IsReadOnly = !await CanEditAsync(culture),
            Search = search,
            Providers = resources.GroupBy(item => item.Resource.AssemblyName).Select(assembly => new TranslatableStringGroupViewModel
            {
                Name = assembly.Key,
                SubGroups = assembly.GroupBy(item => item.Resource.Context).Select(context => new TranslatableStringSubGroupViewModel
                {
                    Name = context.Key,
                    Strings = context.Select(item => new TranslatableStringViewModel
                    {
                        Context = item.Resource.Context,
                        Key = item.Resource.Key,
                        Plural = item.Resource.Plural,
                        Metadata = item.Metadata,
                        Values = translatedValues.TryGetValue((item.Resource.Context, item.Resource.Key), out var values)
                            ? values : Enumerable.Repeat(string.Empty, item.Resource.Plural == null ? 1 : formCount).ToArray(),
                    }).ToList(),
                }).ToList(),
            }).ToList(),
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
