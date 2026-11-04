using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.RateLimits.ViewModels;

namespace OrchardCore.RateLimits.Drivers;

public sealed class SlidingWindowRateLimiterDisplayDriver : DisplayDriver<RateLimitLimiter>
{
    private readonly IRateLimiterSource _source;
    private readonly IStringLocalizer S;

    public SlidingWindowRateLimiterDisplayDriver(
        [FromKeyedServices(SlidingWindowRateLimiterSource.SourceName)] IRateLimiterSource source,
        IStringLocalizer<SlidingWindowRateLimiterDisplayDriver> stringLocalizer)
    {
        _source = source;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RateLimitLimiter limiter, BuildEditorContext context)
    {
        if (limiter.Source != SlidingWindowRateLimiterSource.SourceName)
        {
            return null;
        }

        return Initialize<SlidingWindowRateLimiterViewModel>("SlidingWindowRateLimiter_Edit", model =>
        {
            var data = limiter.GetOrCreate<SlidingWindowRateLimiterData>();
            model.PermitLimit = data.PermitLimit;
            model.QueueLimit = data.QueueLimit;
            model.WindowSeconds = data.WindowSeconds;
            model.SegmentsPerWindow = data.SegmentsPerWindow;
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(RateLimitLimiter limiter, UpdateEditorContext context)
    {
        if (limiter.Source != SlidingWindowRateLimiterSource.SourceName)
        {
            return null;
        }

        var model = new SlidingWindowRateLimiterViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var data = new SlidingWindowRateLimiterData
        {
            PermitLimit = model.PermitLimit,
            QueueLimit = model.QueueLimit,
            WindowSeconds = model.WindowSeconds,
            SegmentsPerWindow = model.SegmentsPerWindow,
        };

        foreach (var error in RateLimitLimiterValidation.Validate(data, S))
        {
            context.Updater.ModelState.AddModelError(Prefix, error.Key, error.Value);
        }

        limiter.Put(data);

        return Edit(limiter, context);
    }

    public override IDisplayResult Display(RateLimitLimiter limiter, BuildDisplayContext context)
    {
        if (limiter.Source != SlidingWindowRateLimiterSource.SourceName)
        {
            return null;
        }

        return Combine(
            Initialize<RateLimiterSourceViewModel>("SlidingWindowRateLimiter_SummaryAdmin", model =>
            {
                model.Name = _source.Name;
                model.DisplayName = _source.DisplayName.Value;
                model.Description = _source.Describe(limiter);
            }).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:5"));
    }
}
