using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Schemes;
using OrchardCore.Themes.TheAdmin.Services;
using OrchardCore.Themes.TheAdmin.ViewModels;

namespace OrchardCore.Themes.TheAdmin.Controllers;

/// <summary>
/// Lists, adds, edits and deletes the schemes of TheAdmin (its own ones included), sets the site's default scheme, and
/// previews a scheme while it is edited.
/// </summary>
[Admin("TheAdmin/Schemes/{action}/{id?}", "AdminSchemes{action}")]
public sealed class SchemesController : Controller
{
    /// <summary>
    /// The id the editor's previews carry; its rules are the ones <see cref="Preview"/> returns.
    /// </summary>
    public const string PreviewSchemeId = "editor-preview";

    private readonly IAuthorizationService _authorizationService;
    private readonly AdminSchemeStore _store;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemesController"/> class.
    /// </summary>
    public SchemesController(
        IAuthorizationService authorizationService,
        AdminSchemeStore store,
        INotifier notifier,
        IHtmlLocalizer<SchemesController> htmlLocalizer,
        IStringLocalizer<SchemesController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _store = store;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("TheAdmin/Schemes", "AdminSchemesIndex")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var defaultId = await _store.GetDefaultIdAsync();
        var schemes = await _store.ListAsync();

        return View(new AdminSchemesIndexViewModel
        {
            List = new AdminListContext<AdminSchemeEntry>
            {
                Source = [.. schemes.Select(scheme => new AdminSchemeEntry
                {
                    Scheme = scheme,
                    IsDefault = string.Equals(scheme.Id, defaultId, StringComparison.Ordinal),
                })],
            },
        });
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        return View("Edit", new AdminSchemeViewModel
        {
            Position = await _store.NextPositionAsync(),
        });
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(AdminSchemeViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        model.Id = null;

        ValidateTypography(model);

        if (!ModelState.IsValid)
        {
            return View("Edit", model);
        }

        var scheme = ToScheme(model, AdminSchemeStore.NewId());
        await _store.SaveAsync(scheme);
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was added.", scheme.Name]);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(id);

        if (scheme is null)
        {
            return NotFound();
        }

        return View(new AdminSchemeViewModel
        {
            Id = scheme.Id,
            Name = scheme.Name,
            Position = scheme.Position,
            Primary = scheme.Primary,
            Deep = scheme.Deep,
            Ink = scheme.Ink,
            Accent = scheme.Accent,
            LightMenu = scheme.LightMenu,
            HorizontalFormLayout = scheme.HorizontalFormLayout,
            Hue = scheme.Hue,
            Saturation = scheme.Saturation,
            OnPrimary = scheme.OnPrimary,
            Page = scheme.Page,
            Toolbar = scheme.Toolbar,
            Tint = scheme.Tint,
            TintDark = scheme.TintDark,
            // The fonts the theme ships show by their names.
            // Empty for the theme's font, which the field shows as its placeholder.
            FontFamily = AdminSchemeTypography.GetDisplayName(scheme.FontFamily, TheAdminSchemes.Fonts),
            HeadingFontFamily = AdminSchemeTypography.GetDisplayName(scheme.HeadingFontFamily, TheAdminSchemes.Fonts),
            FontScale = scheme.FontScale ?? AdminSchemeTypography.DefaultFontScale,
            RadiusScale = scheme.RadiusScale ?? AdminSchemeTypography.DefaultRadiusScale,
            Density = scheme.Density ?? AdminSchemeTypography.DefaultDensity,
        });
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string id, AdminSchemeViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var existing = await _store.FindAsync(id);

        if (existing is null)
        {
            return NotFound();
        }

        model.Id = existing.Id;

        ValidateTypography(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // The cached document is immutable: a new record replaces the stored one.
        var scheme = ToScheme(model, existing.Id);
        await _store.SaveAsync(scheme);
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was saved.", scheme.Name]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(id);

        if (scheme is null)
        {
            return NotFound();
        }

        if (string.Equals(await _store.GetDefaultIdAsync(), scheme.Id, StringComparison.Ordinal))
        {
            await _notifier.ErrorAsync(H["The scheme <strong>{0}</strong> is the default scheme and can't be deleted. Set another scheme as the default first.", scheme.Name]);

            return RedirectToAction(nameof(Index));
        }

        if (!await _store.DeleteAsync(id))
        {
            return NotFound();
        }

        // Users who picked it fall back on the site's default: a scheme that is not there is ignored when the page is
        // rendered.
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was deleted.", scheme.Name]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetDefault(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(id);

        if (scheme is null || !await _store.SetDefaultAsync(id))
        {
            return NotFound();
        }

        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> is now the default scheme.", scheme.Name]);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// The CSS of a scheme being edited, for the editor's previews: the same derivation, contrast fixes included, as a
    /// saved scheme. It takes the editor's fields, posted as the form would be.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Preview(AdminSchemeViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
        {
            return Forbid();
        }

        var scheme = ToScheme(model, PreviewSchemeId);

        if (!AdminSchemeStore.HasValidColors(scheme))
        {
            return BadRequest();
        }

        return Content(AdminSchemeStylesheet.Build([scheme]), "text/css; charset=utf-8");
    }

    private static AdminScheme ToScheme(AdminSchemeViewModel model, string id)
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
            HorizontalFormLayout = model.HorizontalFormLayout,
            Hue = model.Hue,
            Saturation = model.Saturation,
            OnPrimary = Optional(model.OnPrimary),
            Page = Optional(model.Page),
            Toolbar = Optional(model.Toolbar),
            Tint = Optional(model.Tint),
            TintDark = Optional(model.TintDark),
            FontFamily = OptionalFont(model.FontFamily),
            HeadingFontFamily = OptionalText(model.HeadingFontFamily),
            FontScale = model.FontScale,
            RadiusScale = model.RadiusScale,
            Density = model.Density,
        };

    private void ValidateTypography(AdminSchemeViewModel model)
    {
        if (!AdminSchemeTypography.IsValidFontFamily(model.FontFamily))
        {
            ModelState.AddModelError(nameof(model.FontFamily), S["Enter the name of a font, or several separated by commas. A name holds only letters, digits, spaces, dashes, underscores and dots."]);
        }

        if (!AdminSchemeTypography.IsValidFontFamily(model.HeadingFontFamily))
        {
            ModelState.AddModelError(nameof(model.HeadingFontFamily), S["Enter the name of a font, or several separated by commas, or leave it empty to use the font of the text. A name holds only letters, digits, spaces, dashes, underscores and dots."]);
        }
    }

    // The theme's font is the default: it is stored as no font, so the scheme (and an export of it) names none.
    private static string OptionalFont(string value)
        => AdminSchemeTypography.FindFont(value, TheAdminSchemes.Fonts)?.Key == TheAdminSchemes.DefaultFontFamily ? null : OptionalText(value);

    private static string OptionalText(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Optional(string color)
        => string.IsNullOrWhiteSpace(color) ? null : color.Trim().ToLowerInvariant();
}
