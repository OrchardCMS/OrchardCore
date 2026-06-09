using System.Security.Claims;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Navigation;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.RateLimits.ViewModels;
using OrchardCore.Routing;

namespace OrchardCore.RateLimits.Controllers;

[Admin("RateLimits/{action}/{policyId?}", "RateLimits.{action}")]
public sealed class AdminController : Controller
{
    private static readonly string[] _rateLimiterSourceNames =
    [
        FixedWindowRateLimiterSource.SourceName,
        SlidingWindowRateLimiterSource.SourceName,
        ConcurrencyRateLimiterSource.SourceName,
        TokenBucketRateLimiterSource.SourceName,
    ];

    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotifier _notifier;
    private readonly IRateLimitPolicyStore _policyStore;
    private readonly IRateLimitRouteNameProvider _routeNameProvider;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly RateLimitsOptions _rateLimitsOptions;
    private readonly IDisplayManager<RateLimitLimiter> _displayManager;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        IDisplayManager<RateLimitLimiter> displayManager,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        INotifier notifier,
        IRateLimitPolicyStore policyStore,
        IRateLimitRouteNameProvider routeNameProvider,
        IServiceProvider serviceProvider,
        IShellReleaseManager shellReleaseManager,
        IOptions<RateLimitsOptions> rateLimitsOptions)
    {
        _authorizationService = authorizationService;
        _displayManager = displayManager;
        _notifier = notifier;
        _policyStore = policyStore;
        _routeNameProvider = routeNameProvider;
        _serviceProvider = serviceProvider;
        _shellReleaseManager = shellReleaseManager;
        _rateLimitsOptions = rateLimitsOptions.Value;
        H = htmlLocalizer;
    }

    [Admin("RateLimits", "RateLimits.Index")]
    public async Task<IActionResult> Index(
        string searchText = null,
        PagerParameters pagerParameters = null,
        [FromServices] IOptions<PagerOptions> pagerOptions = null,
        [FromServices] IShapeFactory shapeFactory = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var document = await _policyStore.GetAsync();
        var entries = document.Policies.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            entries = entries.Where(x => SearchMatches(x, searchText));
        }

        var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());
        var orderedEntries = entries
            .OrderBy(x => x.Draft?.Name ?? x.Published?.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var totalCount = orderedEntries.Length;
        var pagedEntries = orderedEntries
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToArray();

        RouteData routeData = null;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            routeData = new RouteData();
            routeData.Values[nameof(searchText)] = searchText;
        }

        var model = new RateLimitsIndexViewModel
        {
            SearchText = searchText,
            Policies =
            [
                .. pagedEntries.Select(BuildPolicyEntry),
            ],
            BuiltInRouteLimits = BuildBuiltInRouteLimits(),
            BulkActions =
            [
                new SelectListItem(H["Publish"].Value, nameof(RateLimitPolicyBulkAction.Publish)),
                new SelectListItem(H["Delete"].Value, nameof(RateLimitPolicyBulkAction.Remove)),
            ],
            Pager = totalCount > pager.PageSize
                ? await shapeFactory.PagerAsync(pager, totalCount, routeData)
                : null,
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        return View(CreateEditViewModel());
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(RateLimitPolicyEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        ValidatePolicy(model, null, await _policyStore.GetAsync());

        if (!ModelState.IsValid)
        {
            model.RouteNames = CreateRouteSelectList(model.RouteName);
            model.PolicyScopes = CreatePolicyScopeList();
            model.LimiterSources = BuildLimiterSources();
            return View(model);
        }

        var policyId = IdGenerator.GenerateId();
        await _policyStore.SaveDraftAsync(policyId, ToPolicy(model));

        await _notifier.SuccessAsync(H["Policy created successfully."]);

        return RedirectToAction(nameof(Edit), new { policyId });
    }

    public async Task<IActionResult> Edit(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var entry = await _policyStore.FindAsync(policyId);
        if (entry is null)
        {
            return NotFound();
        }

        return View(await CreateEditViewModelAsync(RateLimitPolicyStore.Clone(entry)));
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [FormValueRequired("submit.Save")]
    public async Task<IActionResult> EditPost(string policyId, RateLimitPolicyEditViewModel model)
        => await SavePolicyAsync(policyId, model, publish: false);

    [HttpPost]
    [ActionName(nameof(Edit))]
    [FormValueRequired("submit.Publish")]
    public async Task<IActionResult> EditAndPublishPost(string policyId, RateLimitPolicyEditViewModel model)
        => await SavePolicyAsync(policyId, model, publish: true);

    [HttpPost]
    public async Task<IActionResult> Publish(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var entry = await _policyStore.FindAsync(policyId);
        if (entry is null)
        {
            return NotFound();
        }

        await _policyStore.PublishAsync([policyId]);
        _shellReleaseManager.RequestRelease();

        await _notifier.SuccessAsync(H["Policy published successfully."]);

        return RedirectToAction(nameof(Edit), new { policyId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var entry = await _policyStore.FindAsync(policyId);
        if (entry is null)
        {
            return NotFound();
        }

        await _policyStore.DeleteAsync(policyId);

        if (entry.Published is not null)
        {
            _shellReleaseManager.RequestRelease();
        }

        await _notifier.SuccessAsync(H["Policy deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [FormValueRequired("submit.BulkAction")]
    [ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(RateLimitsIndexViewModel model, IEnumerable<string> policyIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var ids = policyIds?.Where(static id => !string.IsNullOrWhiteSpace(id)).ToArray() ?? [];

        if (ids.Length == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        switch (model.BulkAction)
        {
            case RateLimitPolicyBulkAction.Publish:
                await _policyStore.PublishAsync(ids);
                _shellReleaseManager.RequestRelease();
                await _notifier.SuccessAsync(H["Policies published successfully."]);
                break;

            case RateLimitPolicyBulkAction.Remove:
                {
                    var entries = (await _policyStore.GetAsync()).Policies
                        .Where(x => ids.Contains(x.PolicyId, StringComparer.Ordinal))
                        .ToArray();

                    foreach (var entry in entries)
                    {
                        await _policyStore.DeleteAsync(entry.PolicyId);
                    }

                    if (entries.Any(static x => x.Published is not null))
                    {
                        _shellReleaseManager.RequestRelease();
                    }

                    await _notifier.SuccessAsync(H["Policies deleted successfully."]);
                    break;
                }

            default:
                return BadRequest();
        }

        return RedirectToAction(nameof(Index));
    }

    private RateLimitPolicyEntryViewModel BuildPolicyEntry(RateLimitPolicyEntry entry)
    {
        var draft = entry.Draft ?? entry.Published;

        return new RateLimitPolicyEntryViewModel
        {
            PolicyId = entry.PolicyId,
            Policy = draft,
            Name = draft?.Name,
            Description = draft?.Description,
            TargetDescription = DescribeTarget(draft),
            Status = RateLimitPolicyStore.GetStatus(entry),
            PublishedUtc = entry.PublishedUtc,
            HasDraft = entry.Draft is not null,
        };
    }

    private async Task<RateLimitPolicyEditViewModel> CreateEditViewModelAsync(RateLimitPolicyEntry entry)
    {
        var draft = entry.Draft ?? entry.Published ?? new RateLimitPolicy();
        var model = CreateEditViewModel();
        model.PolicyId = entry.PolicyId;
        model.Policy = draft;
        model.Name = draft.Name;
        model.Description = draft.Description;
        model.Scope = draft.Scope;
        model.RouteName = draft.RouteName;
        model.Path = draft.Path;
        model.Status = RateLimitPolicyStore.GetStatus(entry);
        model.PublishedUtc = entry.PublishedUtc;
        model.PublishedTargetDescription = entry.Published is null ? null : DescribeTarget(entry.Published);

        foreach (var limiter in draft.Limiters)
        {
            var shape = await _displayManager.BuildDisplayAsync(limiter, updater: null, displayType: OrchardCoreConstants.DisplayType.SummaryAdmin);
            shape.Properties["PolicyId"] = entry.PolicyId;
            model.DraftLimiters.Add(new ModelEntry<RateLimitLimiter>
            {
                Model = limiter,
                Shape = shape,
            });
        }

        if (entry.Published is not null)
        {
            foreach (var limiter in entry.Published.Limiters)
            {
                model.PublishedLimiters.Add(new ModelEntry<RateLimitLimiter>
                {
                    Model = limiter,
                    Shape = await _displayManager.BuildDisplayAsync(limiter, updater: null, displayType: OrchardCoreConstants.DisplayType.SummaryAdmin),
                });
            }
        }

        return model;
    }

    private async Task<IActionResult> SavePolicyAsync(string policyId, RateLimitPolicyEditViewModel model, bool publish)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var entry = await _policyStore.FindAsync(policyId);
        if (entry is null)
        {
            return NotFound();
        }

        ValidatePolicy(model, policyId, await _policyStore.GetAsync());

        if (!ModelState.IsValid)
        {
            var invalidModel = await CreateEditViewModelAsync(RateLimitPolicyStore.Clone(entry));
            invalidModel.Name = model.Name;
            invalidModel.Description = model.Description;
            invalidModel.Scope = model.Scope;
            invalidModel.RouteName = model.RouteName;
            invalidModel.Path = model.Path;
            return View("Edit", invalidModel);
        }

        var updated = ToPolicy(model, entry.Draft ?? entry.Published);
        updated.Limiters.AddRange(entry.Draft?.Limiters.Select(RateLimitPolicyStore.Clone) ?? []);

        await _policyStore.SaveDraftAsync(policyId, updated);

        if (publish)
        {
            await _policyStore.PublishAsync([policyId]);
            _shellReleaseManager.RequestRelease();
            await _notifier.SuccessAsync(H["Policy saved and published successfully."]);
        }
        else
        {
            await _notifier.SuccessAsync(H["Policy draft updated successfully."]);
        }

        return RedirectToAction(nameof(Edit), new { policyId });
    }

    private RateLimitPolicyEditViewModel CreateEditViewModel()
    {
        return new()
        {
            PolicyScopes = CreatePolicyScopeList(),
            RouteNames = CreateRouteSelectList(),
            LimiterSources = BuildLimiterSources(),
        };
    }

    private void ValidatePolicy(RateLimitPolicyEditViewModel model, string currentPolicyId, RateLimitPolicyDocument document)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), H["The policy name is required."].Value);
        }
        else if (document.Policies.Any(x =>
            !string.Equals(x.PolicyId, currentPolicyId, StringComparison.Ordinal) &&
            string.Equals(x.Draft?.Name ?? x.Published?.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Name), H["A policy with the same name already exists."].Value);
        }

        if (model.Scope == RateLimitPolicyScope.Route && string.IsNullOrWhiteSpace(model.RouteName))
        {
            ModelState.AddModelError(nameof(model.RouteName), H["A route name is required for route policies."].Value);
        }

        if (model.Scope == RateLimitPolicyScope.Endpoint)
        {
            if (string.IsNullOrWhiteSpace(model.Path))
            {
                ModelState.AddModelError(nameof(model.Path), H["A request path is required for endpoint policies."].Value);
            }
            else if (!model.Path.StartsWith('/'))
            {
                ModelState.AddModelError(nameof(model.Path), H["The request path must start with '/'."].Value);
            }
        }
    }

    private RateLimitPolicy ToPolicy(RateLimitPolicyEditViewModel model, RateLimitPolicy existingPolicy = null)
    {
        return new()
        {
            Name = model.Name?.Trim(),
            Description = model.Description?.Trim(),
            OwnerId = existingPolicy?.OwnerId ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            Author = existingPolicy?.Author ?? User.Identity?.Name,
            Scope = model.Scope,
            RouteName = model.Scope == RateLimitPolicyScope.Route ? model.RouteName : null,
            Path = model.Scope == RateLimitPolicyScope.Endpoint ? model.Path?.Trim() : null,
        };
    }

    private IList<SelectListItem> CreatePolicyScopeList()
    {
        return
        [
            new SelectListItem(H["Global"].Value, nameof(RateLimitPolicyScope.Global)),
            new SelectListItem(H["Route"].Value, nameof(RateLimitPolicyScope.Route)),
            new SelectListItem(H["Endpoint"].Value, nameof(RateLimitPolicyScope.Endpoint)),
        ];
    }

    private List<SelectListItem> CreateRouteSelectList(string selectedRouteName = null)
    {
        var routeNames = _routeNameProvider.GetRouteNames().ToList();

        if (!string.IsNullOrWhiteSpace(selectedRouteName) &&
            !routeNames.Contains(selectedRouteName, StringComparer.OrdinalIgnoreCase))
        {
            routeNames.Add(selectedRouteName);
        }

        return
        [
            new SelectListItem(H["Select a route"].Value, string.Empty, string.IsNullOrWhiteSpace(selectedRouteName)),
            .. routeNames
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(routeName => new SelectListItem(routeName, routeName, string.Equals(routeName, selectedRouteName, StringComparison.OrdinalIgnoreCase))),
        ];
    }

    private List<RateLimiterSourceViewModel> BuildLimiterSources()
    {
        return
        [
            .. _rateLimiterSourceNames
                .Select(sourceName => _serviceProvider.GetKeyedService<IRateLimiterSource>(sourceName))
                .Where(static source => source is not null)
                .OrderBy(x => x.DisplayName.Value, StringComparer.OrdinalIgnoreCase)
                .Select(x => new RateLimiterSourceViewModel
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName.Value,
                    Description = x.Description.Value,
                }),
        ];
    }

    private List<BuiltInRouteRateLimitViewModel> BuildBuiltInRouteLimits()
    {
        var routes = _routeNameProvider.GetRoutes().ToLookup(static x => x.Name, StringComparer.OrdinalIgnoreCase);

        return
        [
            .. _rateLimitsOptions.RouteRateLimits
                .OrderBy(x => x.RouteName, StringComparer.OrdinalIgnoreCase)
                .Select(x =>
                {
                    var route = FindMatchingRoute(routes[x.RouteName], x.HttpMethods);

                    return new BuiltInRouteRateLimitViewModel
                    {
                        Path = route?.Path ?? "/" + x.RouteName,
                        RouteName = x.RouteName,
                        Methods = x.HttpMethods.Count == 0 ? [H["Any"].Value] : [.. x.HttpMethods],
                    };
                }),
        ];
    }

    private static RateLimitRouteInfo FindMatchingRoute(IEnumerable<RateLimitRouteInfo> routes, IReadOnlyList<string> methods)
    {
        foreach (var route in routes)
        {
            if (route.Methods.Count == 0 && methods.Count == 0)
            {
                return route;
            }

            if (route.Methods.Count == methods.Count &&
                route.Methods.All(method => methods.Contains(method, StringComparer.OrdinalIgnoreCase)))
            {
                return route;
            }
        }

        return routes.FirstOrDefault();
    }

    private static bool SearchMatches(RateLimitPolicyEntry entry, string searchText)
    {
        var policy = entry.Draft ?? entry.Published;
        if (policy is null)
        {
            return false;
        }

        return Contains(policy.Name, searchText)
            || Contains(policy.Description, searchText)
            || Contains(policy.RouteName, searchText)
            || Contains(policy.Path, searchText);
    }

    private static bool Contains(string value, string searchText)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private string DescribeTarget(RateLimitPolicy policy)
    {
        return policy?.Scope switch
        {
            RateLimitPolicyScope.Global => H["Applies to every tenant request."].Value,
            RateLimitPolicyScope.Route => string.Format(CultureInfo.CurrentCulture, H["Matches the route name '{0}'."].Value, policy.RouteName),
            RateLimitPolicyScope.Endpoint => string.Format(CultureInfo.CurrentCulture, H["Matches requests starting with '{0}'."].Value, policy.Path),
            _ => string.Empty,
        };
    }
}
