using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

/// <summary>
/// Edits the <see cref="ActivityRetryPolicy"/> of a task. A task without retries that faults the instance keeps no
/// policy in its properties.
/// </summary>
public sealed class ActivityRetryPolicyDisplayDriver : DisplayDriver<IActivity>
{
    internal readonly IStringLocalizer S;

    public ActivityRetryPolicyDisplayDriver(IStringLocalizer<ActivityRetryPolicyDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(IActivity activity, BuildEditorContext context)
    {
        if (activity.IsEvent())
        {
            return null;
        }

        return Initialize<ActivityRetryPolicyEditViewModel>("ActivityRetryPolicy_Edit", viewModel =>
        {
            var policy = activity.TryGet<ActivityRetryPolicy>(out var section) && section is not null ? section : new ActivityRetryPolicy();

            viewModel.MaxRetries = policy.MaxRetries;
            viewModel.DelaySeconds = policy.DelaySeconds;
            viewModel.Backoff = policy.Backoff;
            viewModel.OnFailure = policy.OnFailure;
        }).Location("Content:after");
    }

    public override async Task<IDisplayResult> UpdateAsync(IActivity activity, UpdateEditorContext context)
    {
        if (activity.IsEvent())
        {
            return null;
        }

        var viewModel = new ActivityRetryPolicyEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        if (viewModel.MaxRetries < 0 || viewModel.MaxRetries > ActivityRetryPolicy.MaxRetriesLimit)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.MaxRetries), S["Retries must be between 0 and {0}.", ActivityRetryPolicy.MaxRetriesLimit]);
        }

        if (viewModel.DelaySeconds < 0 || viewModel.DelaySeconds > ActivityRetryPolicy.MaxDelay.TotalSeconds)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.DelaySeconds), S["The delay must be between 0 and {0} seconds.", ActivityRetryPolicy.MaxDelay.TotalSeconds]);
        }

        if (!Enum.IsDefined(viewModel.Backoff))
        {
            viewModel.Backoff = ActivityRetryBackoff.Fixed;
        }

        if (!Enum.IsDefined(viewModel.OnFailure))
        {
            viewModel.OnFailure = ActivityFailureBehavior.FaultWorkflow;
        }

        var policy = new ActivityRetryPolicy
        {
            MaxRetries = viewModel.MaxRetries,
            DelaySeconds = viewModel.MaxRetries > 0 ? viewModel.DelaySeconds : 0,
            Backoff = viewModel.MaxRetries > 0 ? viewModel.Backoff : ActivityRetryBackoff.Fixed,
            OnFailure = viewModel.OnFailure,
        };

        if (context.Updater.ModelState.IsValid)
        {
            if (policy.IsActive)
            {
                activity.Put(policy);
            }
            else
            {
                activity.Properties.Remove(nameof(ActivityRetryPolicy));
            }
        }

        return Edit(activity, context);
    }

    protected override void BuildPrefix(IActivity model, string htmlFieldPrefix)
    {
        Prefix = string.IsNullOrEmpty(htmlFieldPrefix) ? nameof(ActivityRetryPolicy) : $"{htmlFieldPrefix}.{nameof(ActivityRetryPolicy)}";
    }
}
