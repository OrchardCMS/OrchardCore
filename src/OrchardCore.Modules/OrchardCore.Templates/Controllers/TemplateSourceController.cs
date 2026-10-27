using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Controllers;

/// <summary>
/// Lists the templates of a template source, e.g. the active theme, to view them and to clone, reset or delete
/// their local overrides.
/// </summary>
[Admin("TemplateSources/{source}/{action}", "TemplateSources.{action}")]
public sealed class TemplateSourceController : Controller
{
    private const string _optionsSearch = "Options.Search";
    private const string _optionsFilter = "Options.Filter";
    private const string _optionsAdminTemplates = "Options.AdminTemplates";
    private const string _optionsScope = "Options.Scope";

    private readonly IAuthorizationService _authorizationService;
    private readonly SourceTemplatesService _sourceTemplatesService;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public TemplateSourceController(
        IAuthorizationService authorizationService,
        SourceTemplatesService sourceTemplatesService,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        INotifier notifier,
        IStringLocalizer<TemplateSourceController> stringLocalizer,
        IHtmlLocalizer<TemplateSourceController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _sourceTemplatesService = sourceTemplatesService;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("TemplateSources/{source}", "TemplateSources.Index")]
    public async Task<IActionResult> Index(string source, SourceTemplatesOptions options, PagerParameters pagerParameters)
    {
        if (!await IsAuthorizedAsync(options.AdminTemplates))
        {
            return Forbid();
        }

        var templateSource = _sourceTemplatesService.GetSource(source);

        if (templateSource == null)
        {
            return NotFound();
        }

        var scopes = (await templateSource.GetScopesAsync(options.AdminTemplates)).ToList();

        // An unknown scope, e.g. a theme that was removed, lists the scope in use.
        if (!scopes.Any(scope => string.Equals(scope.Name, options.Scope, StringComparison.OrdinalIgnoreCase)))
        {
            options.Scope = scopes.FirstOrDefault(scope => scope.IsInUse)?.Name;
        }

        IEnumerable<SourceTemplateEntry> entries = await _sourceTemplatesService.GetEntriesAsync(templateSource, options.AdminTemplates, options.Scope);

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            entries = entries.Where(entry =>
                entry.Template.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase) ||
                entry.Template.RelativePath.Contains(options.Search, StringComparison.OrdinalIgnoreCase));
        }

        entries = options.Filter switch
        {
            SourceTemplatesFilter.NotOverridden => entries.Where(entry => !entry.IsOverridden),
            SourceTemplatesFilter.Overridden => entries.Where(entry => entry.IsOverridden),
            SourceTemplatesFilter.Modified => entries.Where(entry => entry.IsModified),
            SourceTemplatesFilter.SourceChanged => entries.Where(entry => entry.IsSourceChanged),
            _ => entries,
        };

        var filteredEntries = entries.ToList();
        var pager = new Pager(pagerParameters, _pagerOptions);

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();
        routeData.Values.TryAdd(_optionsAdminTemplates, options.AdminTemplates);

        if (!string.IsNullOrEmpty(options.Scope))
        {
            routeData.Values.TryAdd(_optionsScope, options.Scope);
        }

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        if (options.Filter != SourceTemplatesFilter.All)
        {
            routeData.Values.TryAdd(_optionsFilter, options.Filter);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, filteredEntries.Count, routeData);

        options.Filters =
        [
            new SelectListItem(S["All templates"], nameof(SourceTemplatesFilter.All)),
            new SelectListItem(S["Not overridden"], nameof(SourceTemplatesFilter.NotOverridden)),
            new SelectListItem(S["Overridden"], nameof(SourceTemplatesFilter.Overridden)),
            new SelectListItem(S["Edited since cloned"], nameof(SourceTemplatesFilter.Modified)),
            new SelectListItem(S["Source changed since cloned"], nameof(SourceTemplatesFilter.SourceChanged)),
        ];

        var currentScope = scopes.FirstOrDefault(scope => string.Equals(scope.Name, options.Scope, StringComparison.OrdinalIgnoreCase));

        options.Scopes = scopes
            .Select(scope => new SelectListItem(scope.IsInUse ? S["{0} (active)", scope.DisplayName] : scope.DisplayName, scope.Name))
            .ToList();

        // The templates of a scope not in use, e.g. a theme that is not active, can't be cloned or reset.
        options.BulkActions = currentScope?.IsInUse != false
            ?
            [
                new SelectListItem(S["Clone"], nameof(SourceTemplatesBulkAction.Clone)),
                new SelectListItem(S["Reset to source"], nameof(SourceTemplatesBulkAction.Reset)),
                new SelectListItem(S["Delete override"], nameof(SourceTemplatesBulkAction.Remove)),
            ]
            :
            [
                new SelectListItem(S["Delete override"], nameof(SourceTemplatesBulkAction.Remove)),
            ];

        var search = await _shapeFactory.CreateAsync("AdminListSearch", Arguments.From(new
        {
            Name = _optionsSearch,
            Value = options.Search,
        }));

        var actions = await _shapeFactory.CreateAsync("TemplateSourceButtons", Arguments.From(new
        {
            Options = options,
        }));

        // The filter renders at the end of the toolbar, next to the bulk actions.
        var filters = await _shapeFactory.CreateAsync("TemplateSourceFilters", Arguments.From(new
        {
            Options = options,
        }));

        var model = new SourceTemplatesIndexViewModel
        {
            Source = templateSource,
            Scope = currentScope,
            Options = options,
            List = new AdminListContext<SourceTemplateEntry>
            {
                Source = filteredEntries.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList(),
                Search = search,
                Actions = actions,
                ToolbarActions = filters,
                Pager = pagerShape,
            },
        };

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(string source, SourceTemplatesIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { nameof(source), source },
            { _optionsAdminTemplates, model.Options.AdminTemplates },
            { _optionsScope, model.Options.Scope },
            { _optionsSearch, model.Options.Search },
            { _optionsFilter, model.Options.Filter },
        });

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> IndexBulkActionPOST(string source, SourceTemplatesOptions options, IEnumerable<string> itemIds)
    {
        if (!await IsAuthorizedAsync(options.AdminTemplates))
        {
            return Forbid();
        }

        var templateSource = _sourceTemplatesService.GetSource(source);

        if (templateSource == null)
        {
            return NotFound();
        }

        var names = new HashSet<string>(itemIds ?? [], StringComparer.OrdinalIgnoreCase);
        var entries = (await _sourceTemplatesService.GetEntriesAsync(templateSource, options.AdminTemplates, options.Scope))
            .Where(entry => names.Contains(entry.Template.Name))
            .ToList();

        var count = 0;

        switch (options.BulkAction)
        {
            case SourceTemplatesBulkAction.None:
                break;
            case SourceTemplatesBulkAction.Clone:
                foreach (var entry in entries.Where(entry => !entry.IsOverridden))
                {
                    count += await _sourceTemplatesService.CloneAsync(templateSource, entry.Template, options.AdminTemplates, overwrite: false) ? 1 : 0;
                }

                await _notifier.SuccessAsync(H["{0} template(s) cloned.", count]);
                break;
            case SourceTemplatesBulkAction.Reset:
                foreach (var entry in entries.Where(entry => entry.IsOverridden))
                {
                    count += await _sourceTemplatesService.CloneAsync(templateSource, entry.Template, options.AdminTemplates, overwrite: true) ? 1 : 0;
                }

                await _notifier.SuccessAsync(H["{0} template(s) reset to their source.", count]);
                break;
            case SourceTemplatesBulkAction.Remove:
                foreach (var entry in entries.Where(entry => entry.IsOverridden))
                {
                    count += await _sourceTemplatesService.DeleteAsync(entry.Template.Name, options.AdminTemplates) ? 1 : 0;
                }

                await _notifier.SuccessAsync(H["{0} template override(s) deleted.", count]);
                break;
            default:
                return BadRequest();
        }

        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { nameof(source), source },
            { _optionsAdminTemplates, options.AdminTemplates },
            { _optionsScope, options.Scope },
        });
    }

    public async Task<IActionResult> Display(string source, string name, bool adminTemplates = false, string scope = null, string returnUrl = null)
    {
        if (!await IsAuthorizedAsync(adminTemplates))
        {
            return Forbid();
        }

        var templateSource = _sourceTemplatesService.GetSource(source);
        var template = templateSource == null ? null : await SourceTemplatesService.FindTemplateAsync(templateSource, name, adminTemplates, scope);

        if (template == null)
        {
            return NotFound();
        }

        var entry = (await _sourceTemplatesService.GetEntriesAsync(templateSource, adminTemplates, scope))
            .First(entry => string.Equals(entry.Template.Name, template.Name, StringComparison.OrdinalIgnoreCase));

        ViewData["ReturnUrl"] = returnUrl;

        return View(new SourceTemplateViewModel
        {
            Source = templateSource,
            Entry = entry,
            Content = await _sourceTemplatesService.ReadAsync(template),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Clone(string source, string name, bool adminTemplates = false, string scope = null, string returnUrl = null)
    {
        if (!await IsAuthorizedAsync(adminTemplates))
        {
            return Forbid();
        }

        var templateSource = _sourceTemplatesService.GetSource(source);
        var template = templateSource == null ? null : await SourceTemplatesService.FindTemplateAsync(templateSource, name, adminTemplates, scope);

        if (template == null)
        {
            return NotFound();
        }

        if (await _sourceTemplatesService.CloneAsync(templateSource, template, adminTemplates, overwrite: false))
        {
            await _notifier.SuccessAsync(H["The \"{0}\" template has been cloned.", template.Name]);
        }
        else
        {
            await _notifier.WarningAsync(H["The \"{0}\" template could not be cloned: only Liquid templates of the active theme without a local override can be cloned.", template.Name]);
        }

        return RedirectToReturnUrlOrIndex(source, adminTemplates, scope, returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Reset(string source, string name, bool adminTemplates = false, string scope = null, string returnUrl = null)
    {
        if (!await IsAuthorizedAsync(adminTemplates))
        {
            return Forbid();
        }

        var templateSource = _sourceTemplatesService.GetSource(source);
        var template = templateSource == null ? null : await SourceTemplatesService.FindTemplateAsync(templateSource, name, adminTemplates, scope);

        if (template == null)
        {
            return NotFound();
        }

        if (await _sourceTemplatesService.CloneAsync(templateSource, template, adminTemplates, overwrite: true))
        {
            await _notifier.SuccessAsync(H["The \"{0}\" template has been reset to its source.", template.Name]);
        }
        else
        {
            await _notifier.WarningAsync(H["The \"{0}\" template could not be reset: only Liquid templates of the active theme can be reset.", template.Name]);
        }

        return RedirectToReturnUrlOrIndex(source, adminTemplates, scope, returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string source, string name, bool adminTemplates = false, string scope = null, string returnUrl = null)
    {
        if (!await IsAuthorizedAsync(adminTemplates))
        {
            return Forbid();
        }

        if (!await _sourceTemplatesService.DeleteAsync(name, adminTemplates))
        {
            return NotFound();
        }

        await _notifier.SuccessAsync(H["The override of the \"{0}\" template has been deleted.", name]);

        return RedirectToReturnUrlOrIndex(source, adminTemplates, scope, returnUrl);
    }

    private IActionResult RedirectToReturnUrlOrIndex(string source, bool adminTemplates, string scope, string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.Redirect(returnUrl, true);
        }

        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { nameof(source), source },
            { _optionsAdminTemplates, adminTemplates },
            { _optionsScope, scope },
        });
    }

    private async Task<bool> IsAuthorizedAsync(bool adminTemplates)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return false;
        }

        var permission = adminTemplates
            ? AdminTemplatesPermissions.ManageAdminTemplates
            : Permissions.ManageTemplates;

        return await _authorizationService.AuthorizeAsync(User, permission);
    }
}
