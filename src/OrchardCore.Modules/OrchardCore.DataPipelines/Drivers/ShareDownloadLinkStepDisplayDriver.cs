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
        model.MaxDownloads = settings.MaxDownloads;
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

        if (model.MaxDownloads < 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.MaxDownloads), S["The number of downloads of a link can't be negative."]);
        }

        settings.ExpiresAfterDays = model.ExpiresAfterDays;
        settings.MaxDownloads = Math.Max(0, model.MaxDownloads);
        settings.Subject = model.Subject?.Trim();
        settings.Message = model.Message?.Trim();

        return ValueTask.CompletedTask;
    }
}

public class ShareDownloadLinkStepViewModel
{
    public string Recipients { get; set; }

    public int ExpiresAfterDays { get; set; }

    public int MaxDownloads { get; set; }

    public string Subject { get; set; }

    public string Message { get; set; }
}
