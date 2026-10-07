using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.RateLimits.ViewModels;

namespace OrchardCore.RateLimits.Controllers;

[Admin("RateLimits/Limiter/{action}/{policyId}/{limiterId?}", "RateLimits.Limiter.{action}")]
public sealed class LimiterController : Controller
{
    private const string PublishedPolicyLimiterMessage = "This policy is enabled and cannot be modified. To modify it, disable it first or create another policy.";

    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<RateLimitLimiter> _displayManager;
    private readonly INotifier _notifier;
    private readonly IRateLimitPolicyStore _policyStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IHtmlLocalizer H;

    public LimiterController(
        IAuthorizationService authorizationService,
        IDisplayManager<RateLimitLimiter> displayManager,
        IHtmlLocalizer<LimiterController> htmlLocalizer,
        INotifier notifier,
        IRateLimitPolicyStore policyStore,
        IServiceProvider serviceProvider,
        IUpdateModelAccessor updateModelAccessor)
    {
        _authorizationService = authorizationService;
        _displayManager = displayManager;
        _notifier = notifier;
        _policyStore = policyStore;
        _serviceProvider = serviceProvider;
        _updateModelAccessor = updateModelAccessor;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Create(string policyId, string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var policy = await GetEditablePolicyAsync(policyId);
        if (policy is null)
        {
            return NotFound();
        }

        if (!CanModifyLimiters(policy))
        {
            await _notifier.WarningAsync(H[PublishedPolicyLimiterMessage]);
            return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
        }

        var source = _serviceProvider.GetKeyedService<IRateLimiterSource>(id);
        if (source is null)
        {
            return NotFound();
        }

        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = source.Name,
        };

        var model = new RateLimiterEditorViewModel
        {
            DisplayName = source.DisplayName.Value,
            Description = source.Description.Value,
            DocumentationUrl = GetLimiterDocumentationUrl(source.Name),
            Editor = await _displayManager.BuildEditorAsync(limiter, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(string policyId, string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var policy = await GetEditablePolicyAsync(policyId);

        if (policy is null)
        {
            return NotFound();
        }

        if (!CanModifyLimiters(policy))
        {
            await _notifier.WarningAsync(H[PublishedPolicyLimiterMessage]);
            return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
        }

        var source = _serviceProvider.GetKeyedService<IRateLimiterSource>(id);
        if (source is null)
        {
            return NotFound();
        }

        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = source.Name,
        };

        var model = new RateLimiterEditorViewModel
        {
            DisplayName = source.DisplayName.Value,
            Description = source.Description.Value,
            DocumentationUrl = GetLimiterDocumentationUrl(source.Name),
            Editor = await _displayManager.UpdateEditorAsync(limiter, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        policy.Limiters.Add(limiter);

        await _policyStore.UpdateAsync(policy);

        await _notifier.SuccessAsync(H["Limiter added successfully."]);

        return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
    }

    public async Task<IActionResult> Edit(string policyId, string limiterId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var (policy, limiter, source) = await GetLimiterAsync(policyId, limiterId);

        if (policy is null || limiter is null || source is null)
        {
            return NotFound();
        }

        if (!CanModifyLimiters(policy))
        {
            await _notifier.WarningAsync(H[PublishedPolicyLimiterMessage]);
            return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
        }

        var model = new RateLimiterEditorViewModel
        {
            DisplayName = source.DisplayName.Value,
            Description = source.Description.Value,
            DocumentationUrl = GetLimiterDocumentationUrl(source.Name),
            Editor = await _displayManager.BuildEditorAsync(limiter, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string policyId, string limiterId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var (policy, originalLimiter, source) = await GetLimiterAsync(policyId, limiterId);
        if (policy is null || originalLimiter is null || source is null)
        {
            return NotFound();
        }

        if (!CanModifyLimiters(policy))
        {
            await _notifier.WarningAsync(H[PublishedPolicyLimiterMessage]);
            return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
        }

        var model = new RateLimiterEditorViewModel
        {
            DisplayName = source.DisplayName.Value,
            Description = source.Description.Value,
            DocumentationUrl = GetLimiterDocumentationUrl(source.Name),
            Editor = await _displayManager.UpdateEditorAsync(originalLimiter, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _policyStore.UpdateAsync(policy);

        await _notifier.SuccessAsync(H["Limiter updated successfully."]);

        return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string policyId, string limiterId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RateLimitsPermissions.ManageRateLimits))
        {
            return Forbid();
        }

        var (policy, limiter, _) = await GetLimiterAsync(policyId, limiterId);
        if (policy is null || limiter is null)
        {
            return NotFound();
        }

        if (!CanModifyLimiters(policy))
        {
            await _notifier.WarningAsync(H[PublishedPolicyLimiterMessage]);
            return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
        }

        policy.Limiters.RemoveAll(x => string.Equals(x.Id, limiterId, StringComparison.Ordinal));
        await _policyStore.UpdateAsync(policy);

        await _notifier.SuccessAsync(H["Limiter removed successfully."]);

        return RedirectToAction(nameof(AdminController.Edit), "Admin", new { policyId });
    }

    private async Task<(RateLimitPolicy Policy, RateLimitLimiter Limiter, IRateLimiterSource Source)> GetLimiterAsync(string policyId, string limiterId)
    {
        var policy = await GetEditablePolicyAsync(policyId);
        if (policy is null)
        {
            return default;
        }

        var limiter = policy.Limiters.FirstOrDefault(x => string.Equals(x.Id, limiterId, StringComparison.Ordinal));
        if (limiter is null)
        {
            return default;
        }

        var source = _serviceProvider.GetKeyedService<IRateLimiterSource>(limiter.Source);
        if (source is null)
        {
            return default;
        }

        return (policy, limiter, source);
    }

    private async Task<RateLimitPolicy> GetEditablePolicyAsync(string policyId)
        => await _policyStore.FindByIdAsync(policyId, PolicyVersion.Current);

    private static bool CanModifyLimiters(RateLimitPolicy policy)
        => !(policy?.IsEnabled ?? false);

    private static string GetLimiterDocumentationUrl(string sourceName)
    {
        var fragment = sourceName switch
        {
            FixedWindowRateLimiterSource.SourceName => "#fixed-window",
            SlidingWindowRateLimiterSource.SourceName => "#sliding-window",
            ConcurrencyRateLimiterSource.SourceName => "#concurrency",
            TokenBucketRateLimiterSource.SourceName => "#token-bucket",
            _ => string.Empty,
        };

        return $"{OrchardCore.Admin.Constants.DocsUrl}reference/modules/RateLimits/{fragment}";
    }
}
