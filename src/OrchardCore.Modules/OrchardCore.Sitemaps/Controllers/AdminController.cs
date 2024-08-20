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
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers;

[Admin("Sitemaps/{action}/{sitemapId?}", "Sitemaps{action}")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly ISitemapHelperService _sitemapService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<SitemapSource> _displayManager;
    private readonly IEnumerable<ISitemapSourceFactory> _sourceFactories;
    private readonly ISitemapManager _sitemapManager;
    private readonly ISitemapIdGenerator _sitemapIdGenerator;
    private readonly PagerOptions _pagerOptions;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISitemapHelperService sitemapService,
        IAuthorizationService authorizationService,
        IDisplayManager<SitemapSource> displayManager,
        IEnumerable<ISitemapSourceFactory> sourceFactories,
        ISitemapManager sitemapManager,
        ISitemapIdGenerator sitemapIdGenerator,
        IOptions<PagerOptions> pagerOptions,
        IUpdateModelAccessor updateModelAccessor,
        INotifier notifier,
        IShapeFactory shapeFactory,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _sitemapService = sitemapService;
        _displayManager = displayManager;
        _sourceFactories = sourceFactories;
        _authorizationService = authorizationService;
        _sitemapManager = sitemapManager;
        _sitemapIdGenerator = sitemapIdGenerator;
        _pagerOptions = pagerOptions.Value;
        _updateModelAccessor = updateModelAccessor;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> List(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var sitemaps = (await _sitemapManager.GetSitemapsAsync())
            .OfType<Sitemap>();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            sitemaps = sitemaps.Where(x => x.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase));
        }

        var count = sitemaps.Count();

        var results = sitemaps
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var model = new ListSitemapViewModel
        {
            Sitemaps = results.Select(sm => new SitemapListEntry { SitemapId = sm.SitemapId, Name = sm.Name, Enabled = sm.Enabled }).ToList(),
            Options = options,
            Pager = await _shapeFactory.PagerAsync(pager, count, routeData)
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(List))]
    [FormValueRequired("submit.Filter")]
    public ActionResult ListFilterPOST(ListSitemapViewModel model)
        => RedirectToAction(nameof(List), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Display(string sitemapId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);

        if (sitemap == null)
        {
            return NotFound();
        }

        var items = new List<dynamic>();
        foreach (var source in sitemap.SitemapSources)
        {
            var item = await _displayManager.BuildDisplayAsync(source, _updateModelAccessor.ModelUpdater, "SummaryAdmin");
            item.Properties["SitemapId"] = sitemap.SitemapId;
            item.Properties["SitemapSource"] = source;
            items.Add(item);
        }

        var thumbnails = new Dictionary<string, dynamic>();
        foreach (var factory in _sourceFactories)
        {
            var source = factory.Create();
            var thumbnail = await _displayManager.BuildDisplayAsync(source, _updateModelAccessor.ModelUpdater, "Thumbnail");
            thumbnail.Properties["SitemapSource"] = source;
            thumbnail.Properties["SitemapSourceType"] = factory.Name;
            thumbnail.Properties["Sitemap"] = sitemap;

            thumbnails.Add(factory.Name, thumbnail);
        }

        var model = new DisplaySitemapViewModel
        {
            Sitemap = sitemap,
            Items = items,
            Thumbnails = thumbnails,
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var model = new CreateSitemapViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSitemapViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(model.Path))
            {
                model.Path = _sitemapService.GetSitemapSlug(model.Name);
            }

            await _sitemapService.ValidatePathAsync(model.Path, _updateModelAccessor.ModelUpdater);
        }

        if (ModelState.IsValid)
        {
            var sitemap = new Sitemap
            {
                SitemapId = _sitemapIdGenerator.GenerateUniqueId(),
                Name = model.Name,
                Path = model.Path,
                Enabled = model.Enabled
            };

            await _sitemapManager.UpdateSitemapAsync(sitemap);

            return RedirectToAction(nameof(List));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> Edit(string sitemapId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var sitemap = (await _sitemapManager.GetSitemapAsync(sitemapId)) as Sitemap;

        if (sitemap == null)
        {
            return NotFound();
        }

        var model = new EditSitemapViewModel
        {
            SitemapId = sitemap.SitemapId,
            Name = sitemap.Name,
            Enabled = sitemap.Enabled,
            Path = sitemap.Path
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditSitemapViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var sitemap = (await _sitemapManager.LoadSitemapAsync(model.SitemapId)) as Sitemap;

        if (sitemap == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(model.Path))
            {
                model.Path = _sitemapService.GetSitemapSlug(model.Name);
            }

            await _sitemapService.ValidatePathAsync(model.Path, _updateModelAccessor.ModelUpdater, model.SitemapId);
        }

        if (ModelState.IsValid)
        {
            sitemap.Name = model.Name;
            sitemap.Enabled = model.Enabled;
            sitemap.Path = model.Path;

            await _sitemapManager.UpdateSitemapAsync(sitemap);

            await _notifier.SuccessAsync(H["Sitemap updated successfully."]);

            return RedirectToAction(nameof(List));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string sitemapId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var sitemap = await _sitemapManager.LoadSitemapAsync(sitemapId);

        if (sitemap == null)
        {
            return NotFound();
        }

        await _sitemapManager.DeleteSitemapAsync(sitemapId);

        await _notifier.SuccessAsync(H["Sitemap deleted successfully."]);

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(string sitemapId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        var sitemap = await _sitemapManager.LoadSitemapAsync(sitemapId);

        if (sitemap == null)
        {
            return NotFound();
        }

        sitemap.Enabled = !sitemap.Enabled;

        await _sitemapManager.UpdateSitemapAsync(sitemap);

        await _notifier.SuccessAsync(H["Sitemap toggled successfully."]);

        return RedirectToAction(nameof(List));
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPost(ViewModels.ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var sitemapsList = await _sitemapManager.LoadSitemapsAsync();
            var checkedContentItems = sitemapsList.Where(x => itemIds.Contains(x.SitemapId));
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _sitemapManager.DeleteSitemapAsync(item.SitemapId);
                    }
                    await _notifier.SuccessAsync(H["Sitemaps successfully removed."]);
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(List));
    }
}
