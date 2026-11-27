using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.RateLimits.ViewModels;

namespace OrchardCore.RateLimits.Drivers;

public sealed class FixedWindowRateLimiterDisplayDriver : DisplayDriver<RateLimitLimiter>
{
    private readonly IRateLimiterSource _source;
    private readonly IStringLocalizer S;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;

    public FixedWindowRateLimiterDisplayDriver(
        [FromKeyedServices(FixedWindowRateLimiterSource.SourceName)] IRateLimiterSource source,
        IStringLocalizerFactory stringLocalizerFactory,
        IStringLocalizer<FixedWindowRateLimiterDisplayDriver> stringLocalizer)
    {
        _source = source;
        _stringLocalizerFactory = stringLocalizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RateLimitLimiter limiter, BuildEditorContext context)
    {
        if (limiter.Source != FixedWindowRateLimiterSource.SourceName)
        {
            return null;
        }

        return Initialize<FixedWindowRateLimiterViewModel>("FixedWindowRateLimiter_Edit", model =>
        {
            var data = limiter.GetOrCreate<FixedWindowRateLimiterData>();
            model.PermitLimit = data.PermitLimit;
            model.QueueLimit = data.QueueLimit;
            model.WindowSeconds = data.WindowSeconds;
        }).Location(l => l.Zone("Content", "5").Card(S["Limits"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(RateLimitLimiter limiter, UpdateEditorContext context)
    {
        if (limiter.Source != FixedWindowRateLimiterSource.SourceName)
        {
            return null;
        }

        var model = new FixedWindowRateLimiterViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.PermitLimit < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.PermitLimit), S["Permit limit must be greater than zero."]);
        }

        if (model.WindowSeconds < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.WindowSeconds), S["Window must be greater than zero."]);
        }

        limiter.Put(new FixedWindowRateLimiterData
        {
            PermitLimit = model.PermitLimit,
            QueueLimit = model.QueueLimit,
            WindowSeconds = model.WindowSeconds,
        });

        return Edit(limiter, context);
    }

    public override IDisplayResult Display(RateLimitLimiter limiter, BuildDisplayContext context)
    {
        if (limiter.Source != FixedWindowRateLimiterSource.SourceName)
        {
            return null;
        }

        return Combine(
            Initialize<RateLimiterSourceViewModel>("FixedWindowRateLimiter_SummaryAdmin", model =>
            {
                model.Name = _source.Name;
                model.DisplayName = _stringLocalizerFactory.Localize(FixedWindowRateLimiterSource.DisplayName).Value;
                model.Description = _source.Describe(limiter);
            }).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:5"));
    }
}
