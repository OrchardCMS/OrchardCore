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
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using Parlot;
using YesSql.Filters.Enumerable;

namespace OrchardCore.Shortcodes.Controllers;

[Feature("OrchardCore.Shortcodes.Templates")]
public sealed class AdminController : Controller, IUpdateModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ShortcodeTemplatesManager _shortcodeTemplatesManager;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly IDisplayManager<ShortcodeFilter> _contentOptionsDisplayManager;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IServiceProvider serviceProvider,
        IAuthorizationService authorizationService,
        ShortcodeTemplatesManager shortcodeTemplatesManager,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<PagerOptions> pagerOptions,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IHtmlSanitizerService htmlSanitizerService,
        IDisplayManager<ShortcodeFilter> contentOptionsDisplayManager
    )
    {
        _authorizationService = authorizationService;
        _shortcodeTemplatesManager = shortcodeTemplatesManager;
        _liquidTemplateManager = liquidTemplateManager;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
        _htmlSanitizerService = htmlSanitizerService;
        _contentOptionsDisplayManager = contentOptionsDisplayManager;
        _serviceProvider = serviceProvider;
    }

    [Admin("Shortcodes", "Shortcodes.Index")]
    public async Task<IActionResult> Index(
        [FromServices] IShapeFactory shapeFactory,
        //[FromServices] IContentsAdminListQueryService shortcodesAdminListQueryService,
        [ModelBinder(BinderType = typeof(ShortcodeFilterEngineModelBinder), Name = "q")]
            EnumerableFilterResult<DataSourceEntry> enumerableFilterResult,
        ContentOptionsViewModel options,
        PagerParameters pagerParameters
    )
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var shortcodeTemplatesDocument =
            await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

        var shortcodeTemplates = shortcodeTemplatesDocument.ShortcodeTemplates.ToList();

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            shortcodeTemplates = shortcodeTemplates
                .Where(x => x.Key.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var count = shortcodeTemplates.Count;

        shortcodeTemplates = shortcodeTemplates
            .OrderBy(x => x.Value.ModifiedUtc)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

        /*         // Maintain previous route data when generating page links.
                var routeData = new RouteData();

                if (!string.IsNullOrEmpty(options.SearchText))
                {
                    routeData.Values.TryAdd(_optionsSearch, options.SearchText);
                } */

/*         // The search text is provided back to the UI.
        //options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;

        // Populate route values to maintain previous route data when generating page links.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString()); */



        var pagerShape = await shapeFactory.PagerAsync(pager, count, options.RouteValues);

        var shortcodeFilter = new ShortcodeFilter { FilterResult = enumerableFilterResult, };

        var shortcodeEntries = shortcodeTemplates
            .Select(x => new DataSourceEntry(x.Value))
            .ToList();

        var shortcodes = (
            await shortcodeFilter.FilterResult.ExecuteAsync(
                new ShortcodeQueryContext(_serviceProvider, shortcodeEntries)
            )
        ).ToList();

        // The search text is provided back to the UI.
        shortcodeFilter.SearchText = shortcodeFilter.FilterResult.ToString();
        shortcodeFilter.OriginalSearchText = shortcodeFilter.SearchText;

        // Populate route values to maintain previous route data when generating page links.
        shortcodeFilter.RouteValues["q"] = shortcodeFilter.FilterResult.ToString();
        shortcodeFilter.AllItems = shortcodeEntries;

        shortcodeFilter.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        shortcodeFilter.ContentSorts =
        [
            new SelectListItem(S["Recently created"], nameof(ContentsOrder.Created)),
            new SelectListItem(S["Recently modified"], nameof(ContentsOrder.Modified)),
            new SelectListItem(S["Title"], nameof(ContentsOrder.Title)),
        ];

        var header = await _contentOptionsDisplayManager.BuildEditorAsync(
            shortcodeFilter,
            this,
            false,
            string.Empty,
            string.Empty
        );

        var shapeViewModel = await shapeFactory.CreateAsync<ShortcodeTemplateIndexViewModel>(
            "ShortcodesAdminList",
            viewModel =>
            {
                viewModel.ShortcodeTemplates = shortcodeTemplates
                    .Select(x => new ShortcodeTemplateEntry
                    {
                        Name = x.Key,
                        ShortcodeTemplate = x.Value,
                    })
                    .ToList();
                viewModel.Pager = pagerShape;
                viewModel.Options = options;
                viewModel.Header = header;
            }
        );

        return View(shapeViewModel);
    }

    /*     [HttpPost, ActionName(nameof(Index))]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(ShortcodeTemplateIndexViewModel model) =>
            RedirectToAction(
                nameof(Index),
                new RouteValueDictionary { { _optionsSearch, model.Options.SearchText }, }
            ); */

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ListFilterPOST(ShortcodeFilter filter)
    {
        // When the user has typed something into the search input no further evaluation of the form post is required.
        if (
            !string.Equals(
                filter.SearchText,
                filter.OriginalSearchText,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return RedirectToAction(
                nameof(Index),
                new RouteValueDictionary { { "q", filter.SearchText }, }
            );
        }

        // Evaluate the values provided in the form post and map them to the filter result and route values.
        await _contentOptionsDisplayManager.UpdateEditorAsync(
            filter,
            this,
            false,
            string.Empty,
            string.Empty
        );

        // The route value must always be added after the editors have updated the models.
        filter.RouteValues.TryAdd("q", filter.FilterResult.ToString());

        return RedirectToAction(nameof(Index), filter.RouteValues);
    }

    [Admin("Shortcodes/Create", "Shortcodes.Create")]
    public async Task<IActionResult> Create()
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        return View(new ShortcodeTemplateViewModel());
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(ShortcodeTemplateViewModel model, string submit)
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Name),
                    S["The name is mandatory."]
                );
            }
            else if (!IsValidShortcodeName(model.Name))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Name),
                    S["The name contains invalid characters."]
                );
            }
            else
            {
                var shortcodeTemplatesDocument =
                    await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

                if (shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(model.Name))
                {
                    ModelState.AddModelError(
                        nameof(ShortcodeTemplateViewModel.Name),
                        S["A template with the same name already exists."]
                    );
                }
            }

            if (string.IsNullOrEmpty(model.Content))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Content),
                    S["The template content is mandatory."]
                );
            }
            else if (!_liquidTemplateManager.Validate(model.Content, out var errors))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Content),
                    S[
                        "The template doesn't contain a valid Liquid expression. Details: {0}",
                        string.Join(" ", errors)
                    ]
                );
            }
        }

        if (ModelState.IsValid)
        {
            var template = new ShortcodeTemplate
            {
                Content = model.Content,
                Hint = model.Hint,
                Usage = _htmlSanitizerService.Sanitize(model.Usage),
                DefaultValue = model.DefaultValue,
                Categories = JConvert.DeserializeObject<string[]>(model.SelectedCategories),
                CreatedUtc = DateTime.UtcNow,
                ModifiedUtc = DateTime.UtcNow,
            };

            await _shortcodeTemplatesManager.UpdateShortcodeTemplateAsync(model.Name, template);

            if (submit == "SaveAndContinue")
            {
                return RedirectToAction(nameof(Edit), new { name = model.Name });
            }

            return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [Admin("Shortcodes/Edit/{name}", "Shortcodes.Edit")]
    public async Task<IActionResult> Edit(string name)
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        var shortcodeTemplatesDocument =
            await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

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
    public async Task<IActionResult> Edit(
        string sourceName,
        ShortcodeTemplateViewModel model,
        string submit
    )
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        var shortcodeTemplatesDocument =
            await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();

        if (!shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(sourceName))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Name),
                    S["The name is mandatory."]
                );
            }
            else if (!IsValidShortcodeName(model.Name))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Name),
                    S["The name contains invalid characters."]
                );
            }
            else if (
                !string.Equals(model.Name, sourceName, StringComparison.OrdinalIgnoreCase)
                && shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(model.Name)
            )
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Name),
                    S["A template with the same name already exists."]
                );
            }

            if (string.IsNullOrEmpty(model.Content))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Content),
                    S["The template content is mandatory."]
                );
            }
            else if (!_liquidTemplateManager.Validate(model.Content, out var errors))
            {
                ModelState.AddModelError(
                    nameof(ShortcodeTemplateViewModel.Content),
                    S[
                        "The template doesn't contain a valid Liquid expression. Details: {0}",
                        string.Join(" ", errors)
                    ]
                );
            }
        }

        if (ModelState.IsValid)
        {
            var template = new ShortcodeTemplate
            {
                Content = model.Content,
                Hint = model.Hint,
                Usage = _htmlSanitizerService.Sanitize(model.Usage),
                DefaultValue = model.DefaultValue,
                Categories = JConvert.DeserializeObject<string[]>(model.SelectedCategories),
                CreatedUtc = model.CreatedUtc ?? DateTime.UtcNow, // For adding a created date if not set
                ModifiedUtc = DateTime.UtcNow,
            };

            await _shortcodeTemplatesManager.RemoveShortcodeTemplateAsync(sourceName);

            await _shortcodeTemplatesManager.UpdateShortcodeTemplateAsync(model.Name, template);

            if (submit == "SaveAndContinue")
            {
                return RedirectToAction(nameof(Edit), new { name = model.Name });
            }

            return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form

        // If the name was changed or removed, prevent a 404 or a failure on the next post.
        model.Name = sourceName;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        var shortcodeTemplatesDocument =
            await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();

        if (!shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(name))
        {
            return NotFound();
        }

        await _shortcodeTemplatesManager.RemoveShortcodeTemplateAsync(name);

        await _notifier.SuccessAsync(H["Shortcode template deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(
        ContentOptionsViewModel options,
        IEnumerable<string> itemIds
    )
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                User,
                ShortcodesPermissions.ManageShortcodeTemplates
            )
        )
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var shortcodeTemplatesDocument =
                await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();
            var checkedContentItems = shortcodeTemplatesDocument.ShortcodeTemplates.Where(x =>
                itemIds.Contains(x.Key)
            );
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
                    throw new ArgumentOutOfRangeException(
                        options.BulkAction.ToString(),
                        "Invalid bulk action."
                    );
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private static bool IsValidShortcodeName(string name)
    {
        var scanner = new Scanner(name);
        return scanner.ReadIdentifier(out var result) && name.Length == result.Length;
    }
}
