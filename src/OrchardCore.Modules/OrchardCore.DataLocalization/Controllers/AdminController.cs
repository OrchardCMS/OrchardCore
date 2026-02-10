using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.DataLocalization.ViewModels;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using Constants = OrchardCore.Localization.Data.Constants;

namespace OrchardCore.DataLocalization.Controllers;

[Admin("DataLocalization/{action}/{id?}", "DataLocalization.{action}")]
public class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILocalizationService _localizationService;
    private readonly IEnumerable<ILocalizationDataProvider> _localizationDataProviders;
    private readonly TranslationsManager _translationsManager;

    protected readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        ILocalizationService localizationService,
        IEnumerable<ILocalizationDataProvider> localizationDataProviders,
        TranslationsManager translationsManager,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _localizationService = localizationService;
        _localizationDataProviders = localizationDataProviders;
        _translationsManager = translationsManager;
        S = stringLocalizer;
    }

    public async Task<IActionResult> Index(string culture = null)
    {
        if (!await HasAnyTranslationPermissionAsync())
        {
            return Forbid();
        }

        var allowedCultures = await GetAllowedCulturesAsync();

        if (allowedCultures.Count == 0)
        {
            return Forbid();
        }

        // Default to first allowed culture if none specified.
        var firstCulture = allowedCultures[0].Name;
        culture ??= firstCulture;

        // Validate the requested culture is allowed.
        if (!allowedCultures.Any(c => c.Name == culture))
        {
            culture = firstCulture;
        }

        var isReadOnly = !allowedCultures.Any(c => c.Name == culture && c.CanEdit);

        var model = new TranslationEditorViewModel
        {
            CurrentCulture = culture,
            AllowedCultures = allowedCultures,
            IsReadOnly = isReadOnly,
            Providers = await GetTranslatableStringsAsync(culture),
        };

        return View(model);
    }

    public async Task<IActionResult> Statistics()
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ViewDynamicTranslations))
        {
            return Forbid();
        }

        var model = await GetStatisticsAsync();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetStrings(string culture)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ViewDynamicTranslations))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(culture))
        {
            return BadRequest("Culture is required.");
        }

        var providers = await GetTranslatableStringsAsync(culture);

        return Ok(new { culture, providers });
    }

    [HttpGet]
    public async Task<IActionResult> GetStatisticsJson()
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ViewDynamicTranslations))
        {
            return Forbid();
        }

        var statistics = await GetStatisticsAsync();

        return Ok(statistics);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] TranslationUpdateModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Culture))
        {
            return BadRequest("Invalid request.");
        }

        // Check permission for the specific culture.
        if (!await CanEditCultureAsync(model.Culture))
        {
            return Forbid();
        }

        // Filter out empty translations and convert to the internal model.
        var translations = model.Translations
            .Where(t => !string.IsNullOrWhiteSpace(t.Value))
            .Select(t => new Translation
            {
                Context = t.Context,
                Key = t.Key,
                Value = t.Value,
            });

        await _translationsManager.UpdateTranslationAsync(model.Culture, translations);

        return Ok(new
        {
            success = true,
            message = S["Translations saved successfully."].Value,
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllowedCulturesJson()
    {
        var cultures = await GetAllowedCulturesAsync();

        return Ok(cultures);
    }

    private async Task<bool> HasAnyTranslationPermissionAsync()
    {
        // Check if user has view permission or any edit permission.
        if (await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ViewDynamicTranslations) ||
            await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ManageTranslations))
        {
            return true;
        }

        // Check culture-specific DatatLocalizationPermissions.
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
        foreach (var cultureName in supportedCultures)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            var permission = DataLocalizationPermissions.CreateCulturePermission(cultureName, cultureInfo.DisplayName);

            if (await _authorizationService.AuthorizeAsync(User, permission))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> CanEditCultureAsync(string culture)
    {
        // ManageTranslations grants access to all cultures.
        if (await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ManageTranslations))
        {
            return true;
        }

        // Check culture-specific permission.
        var cultureInfo = CultureInfo.GetCultureInfo(culture);
        var permission = DataLocalizationPermissions.CreateCulturePermission(culture, cultureInfo.DisplayName);

        return await _authorizationService.AuthorizeAsync(User, permission);
    }

    private async Task<IList<CultureViewModel>> GetAllowedCulturesAsync()
    {
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();

        var canManageAll = await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ManageTranslations);

        var canView = await _authorizationService.AuthorizeAsync(User, DataLocalizationPermissions.ViewDynamicTranslations);

        var cultures = new List<CultureViewModel>();

        foreach (var cultureName in supportedCultures)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            var displayName = !string.IsNullOrEmpty(cultureInfo.DisplayName)
                ? cultureInfo.DisplayName
                : cultureInfo.NativeName;

            var canEditCulture = canManageAll;

            if (!canEditCulture)
            {
                var permission = DataLocalizationPermissions.CreateCulturePermission(cultureName, displayName);

                canEditCulture = await _authorizationService.AuthorizeAsync(User, permission);
            }

            // Include culture if user can view or edit.
            if (canView || canEditCulture)
            {
                cultures.Add(new CultureViewModel
                {
                    Name = cultureName,
                    DisplayName = displayName,
                    CanEdit = canEditCulture,
                });
            }
        }

        return cultures;
    }

    private async Task<IList<TranslatableStringGroupViewModel>> GetTranslatableStringsAsync(string culture)
    {
        var translationsDocument = await _translationsManager.GetTranslationsDocumentAsync();
        var existingTranslations = translationsDocument.Translations.TryGetValue(culture, out var translations)
            ? translations.ToDictionary(t => $"{t.Context}|{t.Key}", t => t.Value, StringComparer.OrdinalIgnoreCase)
            : [];

        var groups = new List<TranslatableStringGroupViewModel>();
        foreach (var provider in _localizationDataProviders)
        {
            var descriptors = await provider.GetDescriptorsAsync();

            if (!descriptors.Any())
            {
                continue;
            }

            foreach (var primaryGroup in descriptors.GroupBy(d => GetPrimaryContext(d.Context)))
            {
                var existingGroup = groups.FirstOrDefault(g => g.Name == primaryGroup.Key);

                if (existingGroup == null)
                {
                    existingGroup = new TranslatableStringGroupViewModel
                    {
                        Name = primaryGroup.Key,
                        SubGroups = [],
                        Strings = [],
                    };
                    groups.Add(existingGroup);
                }

                foreach (var subGroup in primaryGroup.GroupBy(d => GetSubContext(d.Context)))
                {
                    TranslatableStringSubGroupViewModel existingSubGroup = null;

                    if (subGroup.Key != null)
                    {
                        existingSubGroup = existingGroup.SubGroups.FirstOrDefault(sg => sg.Name == subGroup.Key);

                        if (existingSubGroup == null)
                        {
                            existingSubGroup = new TranslatableStringSubGroupViewModel
                            {
                                Name = subGroup.Key,
                                Strings = [],
                            };
                            existingGroup.SubGroups.Add(existingSubGroup);
                        }
                    }

                    foreach (var descriptor in subGroup)
                    {
                        var key = $"{descriptor.Context}|{descriptor.Name}";
                        existingTranslations.TryGetValue(key, out var translatedValue);

                        var stringViewModel = new TranslatableStringViewModel
                        {
                            Context = descriptor.Context,
                            Key = descriptor.Name,
                            Value = translatedValue ?? string.Empty,
                        };

                        if (existingSubGroup is null)
                        {
                            existingGroup.Strings.Add(stringViewModel);
                        }
                        else
                        {
                            existingSubGroup.Strings.Add(stringViewModel);
                        }
                    }
                }

                // Sort sub-groups alphabetically.
                existingGroup.SubGroups = existingGroup.SubGroups.OrderBy(sg => sg.Name).ToList();
            }
        }

        // Sort groups alphabetically.
        return groups.OrderBy(g => g.Name).ToList();
    }

    private async Task<TranslationStatisticsViewModel> GetStatisticsAsync()
    {
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
        var translationsDocument = await _translationsManager.GetTranslationsDocumentAsync();

        // Get all translatable strings.
        var allDescriptors = new List<DataLocalizedString>();
        foreach (var provider in _localizationDataProviders)
        {
            var descriptors = await provider.GetDescriptorsAsync();
            allDescriptors.AddRange(descriptors);
        }

        var totalStrings = allDescriptors.Count;
        var totalTranslated = 0;

        var statistics = new TranslationStatisticsViewModel
        {
            ByCulture = [],
            ByCategory = new Dictionary<string, IList<CategoryStatisticsViewModel>>(),
        };

        foreach (var cultureName in supportedCultures)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            var displayName = !string.IsNullOrEmpty(cultureInfo.DisplayName)
                ? cultureInfo.DisplayName
                : cultureInfo.NativeName;

            var cultureTranslations = translationsDocument.Translations.TryGetValue(cultureName, out var translations)
                ? translations.ToDictionary(t => $"{t.Context}|{t.Key}", t => t.Value, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>();

            var cultureTranslated = allDescriptors.Count(d => cultureTranslations.TryGetValue($"{d.Context}|{d.Name}", out var value) &&
                !string.IsNullOrWhiteSpace(value));

            totalTranslated += cultureTranslated;

            statistics.ByCulture.Add(new CultureStatisticsViewModel
            {
                Culture = cultureName,
                DisplayName = displayName,
                Total = totalStrings,
                Translated = cultureTranslated,
            });

            // Calculate per-category statistics for this culture (use primary context).
            var categoryStats = allDescriptors
                .GroupBy(d => GetPrimaryContext(d.Context))
                .Select(g => new CategoryStatisticsViewModel
                {
                    Category = g.Key,
                    Total = g.Count(),
                    Translated = g.Count(d => cultureTranslations.TryGetValue($"{d.Context}|{d.Name}", out var value) &&
                        !string.IsNullOrWhiteSpace(value)),
                })
                .OrderBy(c => c.Category)
                .ToList();

            statistics.ByCategory[cultureName] = categoryStats;
        }

        statistics.Overall = new ProgressViewModel
        {
            Total = totalStrings * supportedCultures.Length,
            Translated = totalTranslated,
        };

        return statistics;
    }

    private static string GetPrimaryContext(string context)
    {
        var index = context.IndexOf(Constants.ContextSeparator);

        return index < 0
            ? context
            : context[..index];
    }

    private static string GetSubContext(string context)
    {
        var index = context.IndexOf(Constants.ContextSeparator);

        return index < 0
            ? null
            : context[(index + 1)..];
    }
}
