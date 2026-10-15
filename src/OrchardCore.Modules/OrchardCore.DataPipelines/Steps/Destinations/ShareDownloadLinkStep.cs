using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Shares the files it reads with users through shared links, sent to them by email. A link works only for the
/// recipients, once signed in, until it expires, is used as many times as allowed, or is revoked; only the hash of its
/// token is stored. Available when the Email feature is enabled.
/// </summary>
public sealed class ShareDownloadLinkStep : DataPipelineStepType<ShareDownloadLinkStep, ShareDownloadLinkStepSettings>, IDataPipelineStepType
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

    public static string Name => StepName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ShareDownloadLinkStep>("Share a download link");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<ShareDownloadLinkStep>("Shares files with users through secure, expiring download links.");

    public static DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public static string Icon => "fa-solid fa-link";

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

        if (settings.MaxDownloads < 0)
        {
            context.AddError(S["The number of downloads of a link can't be negative."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var manager = context.Services.GetRequiredService<ISharedLinkManager>();
        var files = context.Services.GetRequiredService<DataPipelineFileLinkHandler>();
        var recipients = settings.Recipients.Where(recipient => !string.IsNullOrWhiteSpace(recipient)).Select(recipient => recipient.Trim()).ToArray();
        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var subject = DataPipelineTemplate.Render(settings.Subject, context.Run, now);

        await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
        {
            var resourceId = await files.StoreAsync(file, context.CancellationToken);
            var result = await manager.CreateAsync(new SharedLinkRequest
            {
                Kind = DataPipelineFileLinkHandler.LinkKind,
                ResourceId = resourceId,
                Title = Path.GetFileName(file.FileName),
                SourceName = context.Run.PipelineName,
                Properties =
                {
                    [DataPipelineFileLinkHandler.ContentTypeProperty] = file.ContentType,
                    [DataPipelineFileLinkHandler.RunIdProperty] = context.Run.RunId,
                },
                Recipients = recipients,
                Lifetime = TimeSpan.FromDays(Math.Clamp(settings.ExpiresAfterDays, 1, MaxDays)),
                MaxUses = Math.Max(0, settings.MaxDownloads),
                NotifyByEmail = true,
                Subject = subject,
                Message = settings.Message,
            }, context.CancellationToken);

            // The link holds the token, so it is sent to the recipients only, never recorded.
            context.AddDelivery(S["Shared '{0}' with {1} until {2}.", file.FileName, string.Join(", ", result.Link.RecipientNames), result.Link.ExpiresUtc.ToString("u", CultureInfo.InvariantCulture)]);

            var missed = result.Link.RecipientNames.Except(result.Notified, StringComparer.Ordinal).ToList();

            if (missed.Count > 0)
            {
                context.LogWarning(S["The link to '{0}' could not be emailed to {1}: they have no email address, or the email failed.", file.FileName, string.Join(", ", missed)]);
            }
        }
    }
}
