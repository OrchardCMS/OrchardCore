using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly RateLimitsOptions _rateLimitsOptions;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IDisplayManager<RateLimitLimiter> _displayManager;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        IDisplayManager<RateLimitLimiter> displayManager,
        INotifier notifier,
        IRateLimitPolicyStore policyStore,
        IServiceProvider serviceProvider,
        IShellReleaseManager shellReleaseManager,
        EndpointDataSource endpointDataSource,
        IOptions<RateLimitsOptions> rateLimitsOptions,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _displayManager = displayManager;
        _notifier = notifier;
        _policyStore = policyStore;
        _serviceProvider = serviceProvider;
        _shellReleaseManager = shellReleaseManager;
        _endpointDataSource = endpointDataSource;
        _rateLimitsOptions = rateLimitsOptions.Value;
        S = stringLocalizer;
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

        IEnumerable<RateLimitPolicy> policies = await GetCurrentPoliciesAsync();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            policies = policies.Where(x => SearchMatches(x, searchText));
        }

        var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());
        var orderedPolicies = policies
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var totalCount = orderedPolicies.Length;
        var pagedPolicies = orderedPolicies
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
                .. pagedPolicies.Select(BuildPolicyEntry),
            ],
            BuiltInRouteLimits = BuildBuiltInRouteLimits(),
            BulkActions =
            [
                new SelectListItem(S["Enable"], nameof(RateLimitPolicyBulkAction.Enable)),
                new SelectListItem(S["Disable"], nameof(RateLimitPolicyBulkAction.Disable)),
                new SelectListItem(S["Delete"], nameof(RateLimitPolicyBulkAction.Remove)),
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

        ValidatePolicy(model, null, await GetCurrentPoliciesAsync());

        if (!ModelState.IsValid)
        {
            model.PolicyScopes = CreatePolicyScopeList();
            model.LimiterSources = BuildLimiterSources();
            model.InitialIsEnabled = false;
            model.EnabledUtc = null;
            return View(model);
        }

        var policy = ToPolicy(model);
        policy.PolicyId = IdGenerator.GenerateId();
        policy.EnabledUtc = model.IsEnabled ? DateTime.UtcNow : null;

        await _policyStore.CreateAsync(policy);

        if (ShouldReloadPolicy(wasEnabled: false, isEnabled: policy.IsEnabled))
        {
            _shellReleaseManager.RequestRelease();
        }

        await _notifier.SuccessAsync(H["Policy created successfully."]);

        return RedirectToAction(nameof(Edit), new { policyId = policy.PolicyId });
    }

    public async Task<IActionResult> Edit(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var policy = await GetCurrentPolicyAsync(policyId);
        if (policy is null)
        {
            return NotFound();
        }

        return View(await CreateEditViewModelAsync(policy));
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string policyId, RateLimitPolicyEditViewModel model)
        => await SavePolicyAsync(policyId, model);

    [HttpPost]
    public async Task<IActionResult> Enable(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var policy = await GetCurrentPolicyAsync(policyId);
        if (policy is null)
        {
            return NotFound();
        }

        if (!policy.IsEnabled && !await _policyStore.SetStatusAsync(policyId, true))
        {
            return NotFound();
        }

        if (!policy.IsEnabled)
        {
            _shellReleaseManager.RequestRelease();
        }

        await _notifier.SuccessAsync(H["Policy enabled successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Disable(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var policy = await GetCurrentPolicyAsync(policyId);
        if (policy is null)
        {
            return NotFound();
        }

        if (policy.IsEnabled && !await _policyStore.SetStatusAsync(policyId, false))
        {
            return NotFound();
        }

        if (policy.IsEnabled)
        {
            _shellReleaseManager.RequestRelease();
        }

        await _notifier.SuccessAsync(H["Policy disabled successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string policyId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var policy = await GetCurrentPolicyAsync(policyId);
        if (policy is null)
        {
            return NotFound();
        }

        await _policyStore.DeleteAsync(policy);

        if (policy.IsEnabled)
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

        var policies = (await GetCurrentPoliciesAsync())
            .Where(x => ids.Contains(x.PolicyId, StringComparer.Ordinal))
            .ToArray();

        switch (model.BulkAction)
        {
            case RateLimitPolicyBulkAction.Enable:
                {
                    var totalChanged = 0;

                    foreach (var policy in policies.Where(x => !x.IsEnabled))
                    {
                        if (await _policyStore.SetStatusAsync(policy.PolicyId, true))
                        {
                            totalChanged++;
                        }
                    }

                    if (totalChanged == 0)
                    {
                        await _notifier.WarningAsync(H["No policies were enabled."]);
                        break;
                    }

                    _shellReleaseManager.RequestRelease();
                    await _notifier.SuccessAsync(H.Plural(totalChanged, "{1} policy was enabled successfully.", "{1} policies were enabled successfully.", totalChanged));

                    break;
                }

            case RateLimitPolicyBulkAction.Disable:
                {
                    var totalChanged = 0;

                    foreach (var policy in policies.Where(x => x.IsEnabled))
                    {
                        if (await _policyStore.SetStatusAsync(policy.PolicyId, false))
                        {
                            totalChanged++;
                        }
                    }

                    if (totalChanged == 0)
                    {
                        await _notifier.WarningAsync(H["No policies were disabled."]);
                        break;
                    }

                    _shellReleaseManager.RequestRelease();
                    await _notifier.SuccessAsync(H.Plural(totalChanged, "{1} policy was disabled successfully.", "{1} policies were disabled successfully.", totalChanged));

                    break;
                }

            case RateLimitPolicyBulkAction.Remove:
                {
                    var totalDeleted = 0;
                    var shouldReload = false;

                    foreach (var policy in policies)
                    {
                        if (await _policyStore.DeleteAsync(policy))
                        {
                            totalDeleted++;
                            shouldReload |= policy.IsEnabled;
                        }
                    }

                    if (totalDeleted == 0)
                    {
                        await _notifier.WarningAsync(H["No policies were deleted."]);
                        break;
                    }

                    if (shouldReload)
                    {
                        _shellReleaseManager.RequestRelease();
                    }

                    await _notifier.SuccessAsync(H.Plural(totalDeleted, "{1} policy was deleted successfully.", "{1} policies were deleted successfully.", totalDeleted));

                    break;
                }

            default:
                return BadRequest();
        }

        return RedirectToAction(nameof(Index));
    }

    private RateLimitPolicyEntryViewModel BuildPolicyEntry(RateLimitPolicy policy)
    {
        return new RateLimitPolicyEntryViewModel
        {
            PolicyId = policy.PolicyId,
            Policy = policy,
            Name = policy.Name,
            Description = policy.Description,
            TargetDescription = DescribeTarget(policy),
            Status = policy.Status,
            EnabledUtc = policy.EnabledUtc,
            IsEnabled = policy.IsEnabled,
        };
    }

    private async Task<RateLimitPolicyEditViewModel> CreateEditViewModelAsync(RateLimitPolicy policy)
    {
        var model = CreateEditViewModel();
        model.PolicyId = policy.PolicyId;
        model.Policy = policy;
        model.Name = policy.Name;
        model.Description = policy.Description;
        model.Scope = policy.Scope;
        model.Path = policy.Path;
        model.IsEnabled = policy.IsEnabled;
        model.InitialIsEnabled = policy.IsEnabled;
        model.Status = policy.Status;
        model.EnabledUtc = policy.EnabledUtc;

        foreach (var limiter in policy.Limiters)
        {
            var shape = await _displayManager.BuildDisplayAsync(limiter, updater: null, displayType: OrchardCoreConstants.DisplayType.SummaryAdmin);
            shape.Properties["PolicyId"] = policy.PolicyId;
            shape.Properties["CanManage"] = true;
            model.Limiters.Add(new ModelEntry<RateLimitLimiter>
            {
                Model = limiter,
                Shape = shape,
            });
        }

        return model;
    }

    private async Task<IActionResult> SavePolicyAsync(string policyId, RateLimitPolicyEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var existingPolicy = await GetCurrentPolicyAsync(policyId);
        if (existingPolicy is null)
        {
            return NotFound();
        }

        ValidatePolicy(model, policyId, await GetCurrentPoliciesAsync());

        if (!ModelState.IsValid)
        {
            var invalidModel = await CreateEditViewModelAsync(existingPolicy);
            invalidModel.Name = model.Name;
            invalidModel.Description = model.Description;
            invalidModel.Scope = model.Scope;
            invalidModel.Path = model.Path;
            invalidModel.IsEnabled = model.IsEnabled;
            invalidModel.Status = RateLimitPolicyStore.GetStatus(model.IsEnabled);
            invalidModel.EnabledUtc = model.IsEnabled ? existingPolicy.EnabledUtc : null;
            return View("Edit", invalidModel);
        }

        var updated = ToPolicy(model, existingPolicy);
        updated.PolicyId = policyId;
        updated.Status = RateLimitPolicyStore.GetStatus(updated.IsEnabled);
        updated.EnabledUtc = updated.IsEnabled
            ? existingPolicy.IsEnabled ? existingPolicy.EnabledUtc ?? DateTime.UtcNow : DateTime.UtcNow
            : null;
        updated.Limiters.AddRange(existingPolicy.Limiters);

        await _policyStore.UpdateAsync(updated);

        if (ShouldReloadPolicy(existingPolicy.IsEnabled, updated.IsEnabled))
        {
            _shellReleaseManager.RequestRelease();
        }

        await _notifier.SuccessAsync(H["Policy saved successfully."]);

        return RedirectToAction(nameof(Index));
    }

    private RateLimitPolicyEditViewModel CreateEditViewModel()
    {
        return new()
        {
            PolicyScopes = CreatePolicyScopeList(),
            LimiterSources = BuildLimiterSources(),
        };
    }

    private void ValidatePolicy(RateLimitPolicyEditViewModel model, string currentPolicyId, IEnumerable<RateLimitPolicy> policies)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), S["The policy name is required."]);
        }
        else if (policies.Any(x =>
            !string.Equals(x.PolicyId, currentPolicyId, StringComparison.Ordinal) &&
            string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Name), S["A policy with the same name already exists."]);
        }

        if (model.Scope != RateLimitPolicyScope.Global && model.Scope != RateLimitPolicyScope.Endpoint)
        {
            ModelState.AddModelError(nameof(model.Scope), S["The selected policy type is invalid."]);
        }

        if (model.Scope == RateLimitPolicyScope.Endpoint)
        {
            if (string.IsNullOrWhiteSpace(model.Path))
            {
                ModelState.AddModelError(nameof(model.Path), S["A request path is required for endpoint policies."]);
            }
            else if (!model.Path.StartsWith('/'))
            {
                ModelState.AddModelError(nameof(model.Path), S["The request path must start with '/'."]);
            }
        }
    }

    private RateLimitPolicy ToPolicy(RateLimitPolicyEditViewModel model, RateLimitPolicy existingPolicy = null)
    {
        return new()
        {
            PolicyId = existingPolicy?.PolicyId,
            Name = model.Name?.Trim(),
            Description = model.Description?.Trim(),
            OwnerId = existingPolicy?.OwnerId ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            Author = existingPolicy?.Author ?? User.Identity?.Name,
            Scope = model.Scope,
            Path = model.Scope == RateLimitPolicyScope.Endpoint ? model.Path?.Trim() : null,
            IsEnabled = model.IsEnabled,
            EnabledUtc = existingPolicy?.EnabledUtc,
        };
    }

    private IList<SelectListItem> CreatePolicyScopeList()
    {
        return
        [
            new SelectListItem(S["Global"], nameof(RateLimitPolicyScope.Global)),
            new SelectListItem(S["Endpoint"], nameof(RateLimitPolicyScope.Endpoint)),
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
        var endpointsByName = _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .ToLookup(
                static endpoint => endpoint.Metadata.GetMetadata<IRouteNameMetadata>()?.RouteName
                    ?? endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName,
                StringComparer.OrdinalIgnoreCase);

        return _rateLimitsOptions.RouteRateLimits
            .OrderBy(x => x.RouteName, StringComparer.OrdinalIgnoreCase)
            .Select(x => new BuiltInRouteRateLimitViewModel
            {
                Path = FormatPath(FindMatchingEndpoint(endpointsByName[x.RouteName], x.HttpMethods))
                    ?? x.RouteName,
                Methods = x.HttpMethods.Count == 0 ? [S["Any"]] : [.. x.HttpMethods],
            }).ToList();
    }

    private static RouteEndpoint FindMatchingEndpoint(IEnumerable<RouteEndpoint> endpoints, IReadOnlyList<string> methods)
    {
        foreach (var endpoint in endpoints)
        {
            var endpointMethods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods?
                .Where(static method => !string.IsNullOrWhiteSpace(method))
                .ToArray() ?? [];

            if (endpointMethods.Length == 0 && methods.Count == 0)
            {
                return endpoint;
            }

            if (endpointMethods.Length == methods.Count &&
                endpointMethods.All(method => methods.Contains(method, StringComparer.OrdinalIgnoreCase)))
            {
                return endpoint;
            }
        }

        return endpoints.FirstOrDefault();
    }

    private static string FormatPath(RouteEndpoint endpoint)
    {
        if (endpoint is null)
        {
            return null;
        }

        var path = endpoint.RoutePattern.RawText;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = string.Concat(endpoint.RoutePattern.PathSegments.Select(FormatSegment));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path.StartsWith('/') ? path : "/" + path;
    }

    private static string FormatSegment(RoutePatternPathSegment segment)
        => "/" + string.Concat(segment.Parts.Select(FormatPart));

    private static string FormatPart(RoutePatternPart part)
    {
        return part switch
        {
            RoutePatternLiteralPart literal => literal.Content,
            RoutePatternSeparatorPart separator => separator.Content,
            RoutePatternParameterPart parameter => "{" + parameter.Name + "}",
            _ => part.ToString(),
        };
    }

    private async Task<List<RateLimitPolicy>> GetCurrentPoliciesAsync()
        => [.. await _policyStore.GetAllAsync(PolicyVersion.Current)];

    private async Task<RateLimitPolicy> GetCurrentPolicyAsync(string policyId)
        => await _policyStore.FindByIdAsync(policyId, PolicyVersion.Current);

    private static bool SearchMatches(RateLimitPolicy policy, string searchText)
    {
        return Contains(policy.Name, searchText)
            || Contains(policy.Description, searchText)
            || Contains(policy.Path, searchText);
    }

    private static bool Contains(string value, string searchText)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldReloadPolicy(bool wasEnabled, bool isEnabled)
        => wasEnabled || isEnabled;

    private string DescribeTarget(RateLimitPolicy policy)
    {
        return policy?.Scope switch
        {
            RateLimitPolicyScope.Global => S["Applies to every tenant request."],
            RateLimitPolicyScope.Endpoint => string.Format(CultureInfo.CurrentCulture, S["Matches requests starting with '{0}'."], policy.Path),
            _ => string.Empty,
        };
    }
}
