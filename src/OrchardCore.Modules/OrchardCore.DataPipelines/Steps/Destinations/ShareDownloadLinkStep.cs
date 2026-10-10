using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Shares the files it reads with users through download links. A link works only for the recipients, once signed
/// in, until it expires or is revoked; only the hash of its token is stored. When the Email feature is enabled, the
/// recipients can receive the link by email.
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

    public override LocalizedString Description => S["Shares files with users through secure, expiring download links."];

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
                NotifyByEmail = settings.NotifyByEmail,
                Subject = settings.Subject,
                Message = settings.Message,
            }, context.CancellationToken);

            // The link holds the token, so it is sent to the recipients only, never recorded.
            context.AddDelivery(S["Shared '{0}' with {1} until {2}.", file.FileName, string.Join(", ", result.SharedFile.RecipientNames), result.SharedFile.ExpiresUtc.ToString("u", CultureInfo.InvariantCulture)]);

            if (settings.NotifyByEmail && result.Notified.Count < recipients.Length)
            {
                context.LogWarning(S["Some recipients were not notified by email: the Email feature may be disabled, or they have no email address."]);
            }
        }
    }
}
