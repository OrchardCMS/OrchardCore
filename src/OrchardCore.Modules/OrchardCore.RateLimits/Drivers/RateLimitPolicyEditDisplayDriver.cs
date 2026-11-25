using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.RateLimits.ViewModels;

namespace OrchardCore.RateLimits.Drivers;

/// <summary>
/// Builds the editor of a rate limit policy, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class RateLimitPolicyEditDisplayDriver : DisplayDriver<RateLimitPolicyEditViewModel>
{
    internal readonly IStringLocalizer S;

    public RateLimitPolicyEditDisplayDriver(IStringLocalizer<RateLimitPolicyEditDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RateLimitPolicyEditViewModel model, BuildEditorContext context)
    {
        var results = new List<IDisplayResult>
        {
            Initialize<RateLimitPolicyEditViewModel>("RateLimitPolicyGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<RateLimitPolicyEditViewModel>("RateLimitPolicyTarget_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Target"], "20")),
        };

        // The limiters are added to a policy once it is created.
        if (!context.IsNew)
        {
            results.Add(Initialize<RateLimitPolicyEditViewModel>("RateLimitPolicyLimiters_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "3").Card(S["Limiters"], "30")));
        }

        return Combine(results);
    }

    public override async Task<IDisplayResult> UpdateAsync(RateLimitPolicyEditViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Description,
            m => m.IsEnabled,
            m => m.Scope,
            m => m.Path,
            m => m.GroupName);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(RateLimitPolicyEditViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(RateLimitPolicyEditViewModel source, RateLimitPolicyEditViewModel model)
    {
        model.PolicyId = source.PolicyId;
        model.Policy = source.Policy;
        model.Name = source.Name;
        model.Description = source.Description;
        model.Scope = source.Scope;
        model.Path = source.Path;
        model.GroupName = source.GroupName;
        model.IsEnabled = source.IsEnabled;
        model.InitialIsEnabled = source.InitialIsEnabled;
        model.Status = source.Status;
        model.EnabledUtc = source.EnabledUtc;
        model.Limiters = source.Limiters;
        model.LimiterSources = source.LimiterSources;
        model.PolicyScopes = source.PolicyScopes;
        model.AvailableGroups = source.AvailableGroups;
        model.AreLimiterChangesAllowed = source.AreLimiterChangesAllowed;
        model.PublishedPolicyLimiterMessage = source.PublishedPolicyLimiterMessage;
    }
}
