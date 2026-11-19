using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;
using OrchardCore.Themes.Schemes.Models;
using OrchardCore.Themes.Schemes.Services;
using OrchardCore.Themes.Schemes.ViewModels;

namespace OrchardCore.Themes.Schemes.Controllers;

/// <summary>
/// Lists, adds, edits and deletes the schemes of a theme (the theme's defaults included), sets the site's default
/// scheme of the theme, and previews a scheme while it is edited. Only the themes that support schemes are served.
/// </summary>
[Feature(ThemeSchemesConstants.FeatureId)]
[Admin("Themes/Schemes/{themeId}/{action}/{id?}", "ThemeSchemes{action}")]
public sealed class SchemesController : Controller
{
    /// <summary>
    /// The id the editor's previews carry; its rules are the ones <see cref="Preview"/> returns.
    /// </summary>
    public const string PreviewSchemeId = "editor-preview";

    private readonly IAuthorizationService _authorizationService;
    private readonly ThemeSchemeStore _store;
    private readonly IExtensionManager _extensionManager;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemesController"/> class.
    /// </summary>
    public SchemesController(
        IAuthorizationService authorizationService,
        ThemeSchemeStore store,
        IExtensionManager extensionManager,
        INotifier notifier,
        IHtmlLocalizer<SchemesController> htmlLocalizer,
        IStringLocalizer<SchemesController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _store = store;
        _extensionManager = extensionManager;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("Themes/Schemes/{themeId}", "ThemeSchemesIndex")]
    public async Task<IActionResult> Index(string themeId)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var defaultId = await _store.GetDefaultIdAsync(themeId);
        var schemes = await _store.ListAsync(themeId);

        return View(new ThemeSchemesIndexViewModel
        {
            ThemeId = themeId,
            ThemeName = GetThemeName(themeId),
            List = new AdminListContext<ThemeSchemeEntry>
            {
                Source = [.. schemes.Select(scheme => new ThemeSchemeEntry
                {
                    ThemeId = themeId,
                    Scheme = scheme,
                    IsDefault = string.Equals(scheme.Id, defaultId, StringComparison.Ordinal),
                })],
            },
        });
    }

    public async Task<IActionResult> Create(string themeId)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        return View("Edit", new ThemeSchemeViewModel
        {
            ThemeId = themeId,
            ThemeName = GetThemeName(themeId),
            Position = await _store.NextPositionAsync(themeId),
        });
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(string themeId, ThemeSchemeViewModel model)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        model.Id = null;
        model.ThemeId = themeId;
        model.ThemeName = GetThemeName(themeId);
        ValidateTypography(model);

        if (!ModelState.IsValid)
        {
            return View("Edit", model);
        }

        var scheme = ToScheme(model, ThemeSchemeStore.NewId());
        await _store.SaveAsync(themeId, scheme);
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was added.", scheme.Name]);

        return RedirectToAction(nameof(Index), new { themeId });
    }

    public async Task<IActionResult> Edit(string themeId, string id)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(themeId, id);

        if (scheme is null)
        {
            return NotFound();
        }

        return View(new ThemeSchemeViewModel
        {
            ThemeId = themeId,
            ThemeName = GetThemeName(themeId),
            Id = scheme.Id,
            Name = scheme.Name,
            Position = scheme.Position,
            Primary = scheme.Primary,
            Deep = scheme.Deep,
            Ink = scheme.Ink,
            Accent = scheme.Accent,
            LightMenu = scheme.LightMenu,
            Hue = scheme.Hue,
            Saturation = scheme.Saturation,
            OnPrimary = scheme.OnPrimary,
            Page = scheme.Page,
            Toolbar = scheme.Toolbar,
            Tint = scheme.Tint,
            TintDark = scheme.TintDark,
            FontFamily = ThemeSchemeTypography.FindFont(scheme.FontFamily)?.Key ?? ThemeSchemeTypography.DefaultFontFamily,
            HeadingFontFamily = ThemeSchemeTypography.FindFont(scheme.HeadingFontFamily)?.Key,
            FontScale = scheme.FontScale ?? ThemeSchemeTypography.DefaultFontScale,
            RadiusScale = scheme.RadiusScale ?? ThemeSchemeTypography.DefaultRadiusScale,
            Density = scheme.Density ?? ThemeSchemeTypography.DefaultDensity,
        });
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string themeId, string id, ThemeSchemeViewModel model)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var existing = await _store.FindAsync(themeId, id);

        if (existing is null)
        {
            return NotFound();
        }

        model.Id = existing.Id;
        model.ThemeId = themeId;
        model.ThemeName = GetThemeName(themeId);
        ValidateTypography(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // The cached document is immutable: a new record replaces the stored one.
        var scheme = ToScheme(model, existing.Id);
        await _store.SaveAsync(themeId, scheme);
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was saved.", scheme.Name]);

        return RedirectToAction(nameof(Index), new { themeId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string themeId, string id)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(themeId, id);

        if (scheme is null)
        {
            return NotFound();
        }

        if (string.Equals(await _store.GetDefaultIdAsync(themeId), scheme.Id, StringComparison.Ordinal))
        {
            await _notifier.ErrorAsync(H["The scheme <strong>{0}</strong> is the default scheme and can't be deleted. Set another scheme as the default first.", scheme.Name]);

            return RedirectToAction(nameof(Index), new { themeId });
        }

        if (!await _store.DeleteAsync(themeId, id))
        {
            return NotFound();
        }

        // Users who picked it fall back on the site's default: a scheme that is not there is ignored when the page is
        // rendered.
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was deleted.", scheme.Name]);

        return RedirectToAction(nameof(Index), new { themeId });
    }

    [HttpPost]
    public async Task<IActionResult> SetDefault(string themeId, string id)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        // The id in the address may differ in case: the rest of the page uses the theme's own spelling.
        themeId = _store.GetThemeId(themeId);

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(themeId, id);

        if (scheme is null || !await _store.SetDefaultAsync(themeId, id))
        {
            return NotFound();
        }

        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> is now the default scheme.", scheme.Name]);

        return RedirectToAction(nameof(Index), new { themeId });
    }

    /// <summary>
    /// The CSS of a scheme being edited, for the editor's previews: the same derivation, contrast fixes included, as a
    /// saved scheme. It takes the editor's fields, posted as the form would be.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Preview(string themeId, ThemeSchemeViewModel model)
    {
        if (!_store.IsRegistered(themeId))
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = ToScheme(model, PreviewSchemeId);

        if (!ThemeSchemeStore.HasValidColors(scheme))
        {
            return BadRequest();
        }

        return Content(_store.BuildStylesheet(themeId, [scheme]), "text/css; charset=utf-8");
    }

    private string GetThemeName(string themeId)
    {
        var extension = _extensionManager.GetExtension(themeId);

        return extension?.Exists == true && !string.IsNullOrEmpty(extension.Manifest?.Name)
            ? extension.Manifest.Name
            : themeId;
    }

    private static ThemeScheme ToScheme(ThemeSchemeViewModel model, string id)
        => new()
        {
            Id = id,
            Name = model.Name?.Trim(),
            Position = model.Position,
            Primary = model.Primary?.ToLowerInvariant(),
            Deep = model.Deep?.ToLowerInvariant(),
            Ink = model.Ink?.ToLowerInvariant(),
            Accent = model.Accent?.ToLowerInvariant(),
            LightMenu = model.LightMenu,
            Hue = model.Hue,
            Saturation = model.Saturation,
            OnPrimary = Optional(model.OnPrimary),
            Page = Optional(model.Page),
            Toolbar = Optional(model.Toolbar),
            Tint = Optional(model.Tint),
            TintDark = Optional(model.TintDark),
            FontFamily = model.FontFamily,
            HeadingFontFamily = string.IsNullOrEmpty(model.HeadingFontFamily) ? null : model.HeadingFontFamily,
            FontScale = model.FontScale,
            RadiusScale = model.RadiusScale,
            Density = model.Density,
        };

    private void ValidateTypography(ThemeSchemeViewModel model)
    {
        if (ThemeSchemeTypography.FindFont(model.FontFamily) is null)
        {
            ModelState.AddModelError(nameof(model.FontFamily), S["Pick one of the fonts."]);
        }

        if (!ThemeSchemeTypography.IsValidFontFamily(model.HeadingFontFamily))
        {
            ModelState.AddModelError(nameof(model.HeadingFontFamily), S["Pick one of the fonts, or the same font as the text."]);
        }
    }

    private static string Optional(string color)
        => string.IsNullOrWhiteSpace(color) ? null : color.Trim().ToLowerInvariant();
}
