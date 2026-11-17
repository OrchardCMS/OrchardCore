using System.Text;
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
/// Lists, adds, edits and deletes the schemes of the admin (the theme's defaults included), and previews a scheme
/// while it is edited.
/// </summary>
[Admin("TheAdmin/Schemes/{action}/{id?}", "TheAdminSchemes{action}")]
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

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
        {
            return Forbid();
        }

        return View(new AdminSchemesIndexViewModel
        {
            List = new AdminListContext<AdminScheme>
            {
                Source = [.. await _store.ListAsync()],
            },
        });
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
        {
            return Forbid();
        }

        return View("Edit", new AdminSchemeViewModel { Position = await _store.NextPositionAsync() });
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(AdminSchemeViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
        {
            return Forbid();
        }

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
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
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
            Hue = scheme.Hue,
            Saturation = scheme.Saturation,
            OnPrimary = scheme.OnPrimary,
            Page = scheme.Page,
            Toolbar = scheme.Toolbar,
            Tint = scheme.Tint,
            TintDark = scheme.TintDark,
            FontFamily = AdminSchemeTypography.FindFont(scheme.FontFamily)?.Key ?? AdminSchemeTypography.DefaultFontFamily,
            HeadingFontFamily = AdminSchemeTypography.FindFont(scheme.HeadingFontFamily)?.Key,
            FontScale = scheme.FontScale ?? AdminSchemeTypography.DefaultFontScale,
            RadiusScale = scheme.RadiusScale ?? AdminSchemeTypography.DefaultRadiusScale,
            Density = scheme.Density ?? AdminSchemeTypography.DefaultDensity,
        });
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string id, AdminSchemeViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
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
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
        {
            return Forbid();
        }

        var scheme = await _store.FindAsync(id);

        if (scheme is null || !await _store.DeleteAsync(id))
        {
            return NotFound();
        }

        // Users who picked it, and the site default if it was this scheme, fall back on their own: a scheme that is
        // not there is ignored when the page is rendered.
        await _notifier.SuccessAsync(H["The scheme <strong>{0}</strong> was deleted.", scheme.Name]);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// The CSS of a scheme being edited, for the editor's previews: the same derivation, contrast fixes included, as a
    /// saved scheme. It takes the editor's fields, posted as the form would be.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Preview(AdminSchemeViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.ManageAdminSettings))
        {
            return Forbid();
        }

        var definition = AdminSchemeStore.ToDefinition(ToScheme(model, PreviewSchemeId));

        if (definition is null)
        {
            return BadRequest();
        }

        var css = new StringBuilder();
        AdminSchemeCssBuilder.Append(css, PreviewSchemeId, definition);

        return Content(css.ToString(), "text/css; charset=utf-8");
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

    private void ValidateTypography(AdminSchemeViewModel model)
    {
        if (AdminSchemeTypography.FindFont(model.FontFamily) is null)
        {
            ModelState.AddModelError(nameof(model.FontFamily), S["Pick one of the fonts."]);
        }

        if (!AdminSchemeTypography.IsValidFontFamily(model.HeadingFontFamily))
        {
            ModelState.AddModelError(nameof(model.HeadingFontFamily), S["Pick one of the fonts, or the same font as the text."]);
        }
    }

    private static string Optional(string color)
        => string.IsNullOrWhiteSpace(color) ? null : color.Trim().ToLowerInvariant();
}
