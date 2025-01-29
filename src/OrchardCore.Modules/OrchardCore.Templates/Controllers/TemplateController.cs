using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Controllers;

[Admin("Templates/{action}/{name?}", "Templates.{action}")]
public sealed class TemplateController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly TemplatesManager _templatesManager;
    private readonly AdminTemplatesManager _adminTemplatesManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public TemplateController(
        IAuthorizationService authorizationService,
        TemplatesManager templatesManager,
        AdminTemplatesManager adminTemplatesManager,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IStringLocalizer<TemplateController> stringLocalizer,
        IHtmlLocalizer<TemplateController> htmlLocalizer,
        INotifier notifier)
    {
        _authorizationService = authorizationService;
        _templatesManager = templatesManager;
        _adminTemplatesManager = adminTemplatesManager;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public Task<IActionResult> Admin(ContentOptions options, PagerParameters pagerParameters)
    {
        options.AdminTemplates = true;

        // Used to provide a different url such that the Admin Templates menu entry doesn't collide with the Templates ones.
        return Index(options, pagerParameters);
    }

    [Admin("Templates", "Templates.Index")]
    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!options.AdminTemplates && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        if (options.AdminTemplates && !await _authorizationService.AuthorizeAsync(User, AdminTemplatesPermissions.ManageAdminTemplates))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var templatesDocument = options.AdminTemplates
            ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
            : await _templatesManager.GetTemplatesDocumentAsync()
            ;

        var templates = templatesDocument.Templates.ToList();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            templates = templates.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var count = templates.Count;

        templates = templates.OrderBy(x => x.Key)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);
        var model = new TemplateIndexViewModel
        {
            Templates = templates.Select(x => new TemplateEntry { Name = x.Key, Template = x.Value }).ToList(),
            Options = options,
            Pager = pagerShape
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        // The 'Admin' action redirect the user to the 'Index' action.
        // To ensure we render the same 'Index' view in both cases, we have to explicitly specify the name of the view that should be rendered.
        return View(nameof(Index), model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(TemplateIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Create(string name = null, bool adminTemplates = false, string returnUrl = null)
    {
        if (!adminTemplates && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        if (adminTemplates && !await _authorizationService.AuthorizeAsync(User, AdminTemplatesPermissions.ManageAdminTemplates))
        {
            return Forbid();
        }

        var model = new TemplateViewModel
        {
            AdminTemplates = adminTemplates,
            Name = name
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(TemplateViewModel model, string submit, string returnUrl = null)
    {
        if (!model.AdminTemplates && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        if (model.AdminTemplates && !await _authorizationService.AuthorizeAsync(User, AdminTemplatesPermissions.ManageAdminTemplates))
        {
            return Forbid();
        }

        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            await ValidateModelAsync(model);
        }

        if (ModelState.IsValid)
        {
            var template = new Template { Content = model.Content, Description = model.Description };

            await (model.AdminTemplates
                ? _adminTemplatesManager.UpdateTemplateAsync(model.Name, template)
                : _templatesManager.UpdateTemplateAsync(model.Name, template)
                );

            await _notifier.SuccessAsync(H["The \"{0}\" template has been created.", model.Name]);

            if (submit == "SaveAndContinue")
            {
                return RedirectToAction(nameof(Edit), new { name = model.Name, adminTemplates = model.AdminTemplates, returnUrl });
            }
            else
            {
                return RedirectToReturnUrlOrIndex(returnUrl);
            }
        }

        // If we got this far, something failed, redisplay form.
        return View(model);
    }

    public async Task<IActionResult> Edit(string name, bool adminTemplates = false, string returnUrl = null)
    {
        if (!adminTemplates && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        if (adminTemplates && !await _authorizationService.AuthorizeAsync(User, AdminTemplatesPermissions.ManageAdminTemplates))
        {
            return Forbid();
        }

        var templatesDocument = adminTemplates
            ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
            : await _templatesManager.GetTemplatesDocumentAsync()
            ;

        if (!templatesDocument.Templates.TryGetValue(name, out var template))
        {
            return RedirectToAction(nameof(Create), new { name, returnUrl });
        }

        var model = new TemplateViewModel
        {
            AdminTemplates = adminTemplates,
            Name = name,
            Content = template.Content,
            Description = template.Description
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string sourceName, TemplateViewModel model, string submit, string returnUrl = null)
    {
        if (!model.AdminTemplates && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        if (model.AdminTemplates && !await _authorizationService.AuthorizeAsync(User, AdminTemplatesPermissions.ManageAdminTemplates))
        {
            return Forbid();
        }

        var templatesDocument = model.AdminTemplates
            ? await _adminTemplatesManager.LoadTemplatesDocumentAsync()
            : await _templatesManager.LoadTemplatesDocumentAsync()
            ;

        if (ModelState.IsValid)
        {
            await ValidateModelAsync(model, templatesDocument, sourceName);
        }

        if (!templatesDocument.Templates.ContainsKey(sourceName))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var template = new Template { Content = model.Content, Description = model.Description };

            await (model.AdminTemplates
                ? _adminTemplatesManager.RemoveTemplateAsync(sourceName)
                : _templatesManager.RemoveTemplateAsync(sourceName)
                );

            await (model.AdminTemplates
                ? _adminTemplatesManager.UpdateTemplateAsync(model.Name, template)
                : _templatesManager.UpdateTemplateAsync(model.Name, template)
                );

            if (submit != "SaveAndContinue")
            {
                return RedirectToReturnUrlOrIndex(returnUrl);
            }
        }

        // If we got this far, something failed, redisplay form.
        ViewData["ReturnUrl"] = returnUrl;

        // If the name was changed or removed, prevent a 404 or a failure on the next post.
        model.Name = sourceName;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name, string returnUrl, bool adminTemplates = false)
    {
        if (!adminTemplates && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        if (adminTemplates && !await _authorizationService.AuthorizeAsync(User, AdminTemplatesPermissions.ManageAdminTemplates))
        {
            return Forbid();
        }

        var templatesDocument = adminTemplates
            ? await _adminTemplatesManager.LoadTemplatesDocumentAsync()
            : await _templatesManager.LoadTemplatesDocumentAsync();

        if (!templatesDocument.Templates.ContainsKey(name))
        {
            return NotFound();
        }

        await (adminTemplates
                ? _adminTemplatesManager.RemoveTemplateAsync(name)
                : _templatesManager.RemoveTemplateAsync(name));

        await _notifier.SuccessAsync(H["Template deleted successfully."]);

        return RedirectToReturnUrlOrIndex(returnUrl);
    }

    [HttpPost, ActionName("Index")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
        {
            return Forbid();
        }

        switch (options.BulkAction)
        {
            case ContentsBulkAction.None:
                break;
            case ContentsBulkAction.Remove:
                if (itemIds != null)
                {
                    var templatesDocument = options.AdminTemplates
                        ? await _adminTemplatesManager.LoadTemplatesDocumentAsync()
                        : await _templatesManager.LoadTemplatesDocumentAsync();
                    var checkedContentItemIds = templatesDocument.Templates.Keys
                        .Intersect(itemIds, StringComparer.OrdinalIgnoreCase);

                    foreach (var id in checkedContentItemIds)
                    {
                        await (options.AdminTemplates
                            ? _adminTemplatesManager.RemoveTemplateAsync(id)
                            : _templatesManager.RemoveTemplateAsync(id));
                    }

                    await _notifier.SuccessAsync(H["Templates successfully removed."]);
                }

                break;
            default:
                return BadRequest();
        }

        return RedirectToAction(options.AdminTemplates ? nameof(Admin) : nameof(Index));
    }

    private IActionResult RedirectToReturnUrlOrIndex(string returnUrl)
    {
        if ((string.IsNullOrEmpty(returnUrl) == false) && (Url.IsLocalUrl(returnUrl)))
        {
            return this.Redirect(returnUrl, true);
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task ValidateModelAsync(TemplateViewModel model, TemplatesDocument templatesDocument = null, string sourceName = null)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(TemplateViewModel.Name), S["The name is mandatory."]);
        }
        else
        {
            templatesDocument ??= model.AdminTemplates
                ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
                : await _templatesManager.GetTemplatesDocumentAsync();

            if (!model.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase) &&
                templatesDocument.Templates.ContainsKey(model.Name))
            {
                ModelState.AddModelError(nameof(TemplateViewModel.Name), S["A template with the same name already exists."]);
            }
        }

        if (string.IsNullOrWhiteSpace(model.Content))
        {
            var placementsLink = Url.ActionLink("Index", "Admin", new { area = "OrchardCore.Placements" });
            var docsLink = "https://docs.orchardcore.net/en/main/reference/modules/Placements/";

            await _notifier.WarningAsync(H["If you left the content empty because you want to hide the shape, use <a href=\"{0}\">Placements</a> instead. See <a href=\"{1}\">the docs</a> for more info about this feature.", placementsLink, docsLink]);

            ModelState.AddModelError(nameof(TemplateViewModel.Content), S["The content is mandatory."]);
        }
    }
}
