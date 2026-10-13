using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Shares the files it reads with users through download links, sent to them by email. A link works only for the
/// recipients, once signed in, until it expires or is revoked; only the hash of its token is stored. Available when the
/// Email feature is enabled.
/// </summary>
public sealed class ShareDownloadLinkStep : DataPipelineStepType<ShareDownloadLinkStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "ShareDownloadLink";

    /// <summary>
    /// The longest a link can work, in days.
    /// </summary>
    public const int MaxDays = 365;

    private readonly IStringLocalizer S;

    public ShareDownloadLinkStep(IStringLocalizer<ShareDownloadLinkStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Share a download link"];

    public override LocalizedString Description
    {
        get
        {
            return S["Shares files with users through secure, expiring download links."];
        }
    }

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-link";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (!settings.Recipients.Any(recipient => !string.IsNullOrWhiteSpace(recipient)))
        {
            context.AddError(S["Enter the users to share the files with."]);
        }

        if (settings.ExpiresAfterDays is < 1 or > MaxDays)
        {
            context.AddError(S["A link works between 1 and {0} days.", MaxDays]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var manager = context.Services.GetRequiredService<IDataPipelineSharedFileManager>();
        var recipients = settings.Recipients.Where(recipient => !string.IsNullOrWhiteSpace(recipient)).Select(recipient => recipient.Trim()).ToArray();

        await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
        {
            var result = await manager.ShareAsync(file, new DataPipelineSharedFileRequest
            {
                Recipients = recipients,
                Lifetime = TimeSpan.FromDays(Math.Clamp(settings.ExpiresAfterDays, 1, MaxDays)),
                Run = context.Run,
                Subject = settings.Subject,
                Message = settings.Message,
            }, context.CancellationToken);

            // The link holds the token, so it is sent to the recipients only, never recorded.
            context.AddDelivery(S["Shared '{0}' with {1} until {2}.", file.FileName, string.Join(", ", result.SharedFile.RecipientNames), result.SharedFile.ExpiresUtc.ToString("u", CultureInfo.InvariantCulture)]);

            var missed = result.SharedFile.RecipientNames.Except(result.Notified, StringComparer.Ordinal).ToList();

            if (missed.Count > 0)
            {
                context.LogWarning(S["The link to '{0}' could not be emailed to {1}: they have no email address, or the email failed.", file.FileName, string.Join(", ", missed)]);
            }
        }
    }
}
