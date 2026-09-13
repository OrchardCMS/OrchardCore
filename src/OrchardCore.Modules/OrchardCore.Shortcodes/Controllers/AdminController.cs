using System.Text.Json;
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
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;

namespace OrchardCore.Shortcodes.Controllers;

[Feature("OrchardCore.Shortcodes.Templates")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly ShortcodeTemplatesManager _shortcodeTemplatesManager;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        ShortcodeTemplatesManager shortcodeTemplatesManager,
        IOptions<PagerOptions> pagerOptions,
        INotifier notifier,
        IShapeFactory shapeFactory,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer
        )
    {
        _authorizationService = authorizationService;
        _shortcodeTemplatesManager = shortcodeTemplatesManager;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("Shortcodes", "Shortcodes.Index")]
    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions);
        var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

        var shortcodeTemplates = shortcodeTemplatesDocument.ShortcodeTemplates.ToList();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            shortcodeTemplates = shortcodeTemplates.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var count = shortcodeTemplates.Count;

        shortcodeTemplates = shortcodeTemplates.OrderBy(x => x.Key)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);

        var model = new ShortcodeTemplateIndexViewModel
        {
            ShortcodeTemplates = shortcodeTemplates.Select(x => new ShortcodeTemplateEntry { Name = x.Key, ShortcodeTemplate = x.Value }).ToList(),
            Options = options,
            Pager = pagerShape,
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(ShortcodeTemplateIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search },
        });

    [Admin("Shortcodes/Create", "Shortcodes.Create")]
    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        return View(new ShortcodeTemplateViewModel());
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(ShortcodeTemplateViewModel model, string submit)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var result = await _shortcodeTemplatesManager.SaveAsync(model.Name, ToTemplate(model));
            if (result.Status == ShortcodeTemplateMutationStatus.Saved)
            {
                return submit == "SaveAndContinue"
                    ? RedirectToAction(nameof(Edit), new { name = result.Name })
                    : RedirectToAction(nameof(Index));
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [Admin("Shortcodes/Edit/{name}", "Shortcodes.Edit")]
    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

        if (!shortcodeTemplatesDocument.ShortcodeTemplates.TryGetValue(name, out var template))
        {
            return RedirectToAction(nameof(Create), new { name });
        }

        var model = new ShortcodeTemplateViewModel
        {
            Name = name,
            Content = template.Content,
            Hint = template.Hint,
            Usage = template.Usage,
            DefaultValue = template.DefaultValue,
            Categories = template.Categories,
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string sourceName, ShortcodeTemplateViewModel model, string submit)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(sourceName))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _shortcodeTemplatesManager.SaveAsync(model.Name, ToTemplate(model), sourceName);
            if (result.Status == ShortcodeTemplateMutationStatus.NotFound)
            {
                return NotFound();
            }
            if (result.Status is ShortcodeTemplateMutationStatus.Saved or ShortcodeTemplateMutationStatus.Existing)
            {
                return submit == "SaveAndContinue"
                    ? RedirectToAction(nameof(Edit), new { name = result.Name })
                    : RedirectToAction(nameof(Index));
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form

        // If the name was changed or removed, prevent a 404 or a failure on the next post.
        model.Name = sourceName;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        if (!await _shortcodeTemplatesManager.RemoveIfExistsAsync(name))
        {
            return NotFound();
        }

        await _notifier.SuccessAsync(H["Shortcode template deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShortcodesPermissions.ManageShortcodeTemplates))
        {
            return Forbid();
        }

        if (itemIds?.Any() == true)
        {
            var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();
            var checkedContentItems = shortcodeTemplatesDocument.ShortcodeTemplates.Where(x => itemIds.Contains(x.Key));
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _shortcodeTemplatesManager.RemoveShortcodeTemplateAsync(item.Key);
                    }
                    await _notifier.SuccessAsync(H["Shortcode templates successfully removed."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.BulkAction.ToString(), "Invalid bulk action.");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private static ShortcodeTemplate ToTemplate(ShortcodeTemplateViewModel model) => new()
    {
        Content = model.Content,
        Hint = model.Hint,
        Usage = model.Usage,
        DefaultValue = model.DefaultValue,
        Categories = JConvert.DeserializeObject<string[]>(model.SelectedCategories),
    };

    private void AddErrors(ShortcodeTemplateMutationResult result)
    {
        if (result.Status is ShortcodeTemplateMutationStatus.Conflict or ShortcodeTemplateMutationStatus.Existing)
        {
            ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["A template with the same name already exists."]);
        }
        foreach (var error in result.Errors)
        {
            var property = error.Key == "name" ? nameof(ShortcodeTemplateViewModel.Name) : nameof(ShortcodeTemplateViewModel.Content);
            foreach (var message in error.Value)
            {
                ModelState.AddModelError(property, message);
            }
        }
    }
}
