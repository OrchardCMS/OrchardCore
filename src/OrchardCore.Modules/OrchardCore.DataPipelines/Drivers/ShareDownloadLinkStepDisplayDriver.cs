using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Drivers;

public sealed class ShareDownloadLinkStepDisplayDriver : DataPipelineStepDisplayDriver<ShareDownloadLinkStepSettings, ShareDownloadLinkStepViewModel>
{
    private static readonly char[] _separators = [',', ';', '\n', '\r'];

    private readonly IStringLocalizer S;

    public ShareDownloadLinkStepDisplayDriver(IStringLocalizer<ShareDownloadLinkStepDisplayDriver> localizer)
    {
        S = localizer;
    }

    protected override string StepName => ShareDownloadLinkStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, ShareDownloadLinkStepSettings settings, ShareDownloadLinkStepViewModel model)
    {
        model.Recipients = string.Join(", ", settings.Recipients);
        model.ExpiresAfterDays = settings.ExpiresAfterDays;
        model.NotifyByEmail = settings.NotifyByEmail;
        model.Subject = settings.Subject;
        model.Message = settings.Message;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, ShareDownloadLinkStepSettings settings, ShareDownloadLinkStepViewModel model, UpdateEditorContext context)
    {
        settings.Recipients = (model.Recipients ?? string.Empty)
            .Split(_separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (model.ExpiresAfterDays is < 1 or > ShareDownloadLinkStep.MaxDays)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.ExpiresAfterDays), S["A link works between 1 and {0} days.", ShareDownloadLinkStep.MaxDays]);
        }

        settings.ExpiresAfterDays = model.ExpiresAfterDays;
        settings.NotifyByEmail = model.NotifyByEmail;
        settings.Subject = model.Subject?.Trim();
        settings.Message = model.Message?.Trim();

        return ValueTask.CompletedTask;
    }
}

public class ShareDownloadLinkStepViewModel
{
    public string Recipients { get; set; }

    public int ExpiresAfterDays { get; set; }

    public bool NotifyByEmail { get; set; }

    public string Subject { get; set; }

    public string Message { get; set; }
}
