using System.Globalization;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Sends one email with every file it reads attached, with the default email provider of the site.
/// </summary>
public sealed class SendByEmailStep : DataPipelineStepType<SendByEmailStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "SendByEmail";

    /// <summary>
    /// The placeholder replaced with the names of the attached files, separated by commas.
    /// </summary>
    public const string FileNamesPlaceholder = "{FileNames}";

    private const long BytesPerMegabyte = 1024 * 1024;

    private static readonly char[] _separators = [',', ';'];

    private readonly IStringLocalizer S;

    public SendByEmailStep(IStringLocalizer<SendByEmailStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Send by email"];

    public override LocalizedString Description => S["Sends the files as the attachments of an email."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-envelope";

    /// <summary>
    /// Splits a list of email addresses separated by commas or semicolons.
    /// </summary>
    /// <param name="addresses">The list.</param>
    /// <returns>The addresses.</returns>
    public static IReadOnlyList<string> SplitAddresses(string addresses)
        => (addresses ?? string.Empty).Split(_separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (SplitAddresses(settings.To).Count == 0)
        {
            context.AddError(S["Enter the email address of at least one recipient."]);
        }

        var invalid = SplitAddresses(settings.To)
            .Concat(SplitAddresses(settings.Cc))
            .Concat(SplitAddresses(settings.Bcc))
            .Where(address => !System.Net.Mail.MailAddress.TryCreate(address, out _))
            .ToList();

        if (invalid.Count > 0)
        {
            context.AddError(S["These email addresses aren't valid: {0}.", string.Join(", ", invalid)]);
        }

        if (settings.MaxAttachmentSizeMegabytes < 1)
        {
            context.AddError(S["The maximum size of the attachments must be at least 1 MB."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var files = new List<DataPipelineFile>();

        await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
        {
            files.Add(file);
        }

        if (files.Count == 0 && !settings.SendWhenEmpty)
        {
            context.LogInformation(S["There are no files to send: no email was sent."]);

            return;
        }

        var size = files.Sum(file => file.Length);
        var maxSize = Math.Max(1, settings.MaxAttachmentSizeMegabytes) * BytesPerMegabyte;

        if (size > maxSize)
        {
            throw new DataPipelineDestinationException(
                S["The files weigh {0} MB, more than the {1} MB the attachments of the email can weigh. Raise the limit, or zip the files.", ((double)size / BytesPerMegabyte).ToString("0.##", CultureInfo.InvariantCulture), settings.MaxAttachmentSizeMegabytes]);
        }

        var emailService = context.Services?.GetService<IEmailService>()
            ?? throw new DataPipelineDestinationException(S["Emails can't be sent: enable the Email feature."]);

        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var fileNames = string.Join(", ", files.Select(file => file.FileName));
        var to = SplitAddresses(settings.To);
        var cc = SplitAddresses(settings.Cc);

        var message = new MailMessage
        {
            To = string.Join(",", to),
            Cc = cc.Count > 0 ? string.Join(",", cc) : null,
            Bcc = SplitAddresses(settings.Bcc) is { Count: > 0 } bcc ? string.Join(",", bcc) : null,
            Subject = Render(settings.Subject, context.Run, now, fileNames),
        };

        if (settings.IsHtmlBody)
        {
            var run = new DataPipelineRunContext
            {
                PipelineId = WebUtility.HtmlEncode(context.Run.PipelineId),
                PipelineName = WebUtility.HtmlEncode(context.Run.PipelineName),
                RunId = WebUtility.HtmlEncode(context.Run.RunId),
            };

            message.HtmlBody = Render(settings.Body, run, now, WebUtility.HtmlEncode(fileNames));
        }
        else
        {
            message.TextBody = Render(settings.Body, context.Run, now, fileNames);
        }

        var streams = new List<Stream>();

        try
        {
            foreach (var file in files)
            {
                var stream = file.OpenRead();
                streams.Add(stream);
                message.Attachments.Add(new MailMessageAttachment { Filename = file.FileName, Stream = stream });
            }

            var result = await emailService.SendAsync(message, cancellationToken: context.CancellationToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(error => error.Message?.Value).Where(error => !string.IsNullOrEmpty(error)));

                throw new DataPipelineDestinationException(S["The email could not be sent: {0}", errors]);
            }
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }

        var recipients = string.Join(", ", to.Concat(cc));

        context.AddDelivery(files.Count == 0
            ? S["Sent an email without attachments to {0}", recipients]
            : S["Sent {0} by email to {1}", string.Join(", ", files.Select(file => $"'{file.FileName}'")), recipients]);
    }

    private static string Render(string template, DataPipelineRunContext run, DateTime now, string fileNames)
        => (DataPipelineTemplate.Render(template, run, now) ?? string.Empty).Replace(FileNamesPlaceholder, fileNames, StringComparison.Ordinal);
}
