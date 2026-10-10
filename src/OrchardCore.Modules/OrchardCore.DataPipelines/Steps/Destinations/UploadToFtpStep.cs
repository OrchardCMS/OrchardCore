using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Uploads the files it reads to a folder of an FTP server, over TLS unless told otherwise.
/// </summary>
public sealed class UploadToFtpStep : DataPipelineStepType<UploadToFtpStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "UploadToFtp";

    private readonly IDataPipelineFtpClientFactory _clientFactory;
    private readonly DataPipelineSecrets _secrets;
    private readonly IStringLocalizer S;

    public UploadToFtpStep(
        IDataPipelineFtpClientFactory clientFactory,
        DataPipelineSecrets secrets,
        IStringLocalizer<UploadToFtpStep> localizer)
    {
        _clientFactory = clientFactory;
        _secrets = secrets;
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Upload to an FTP server"];

    public override LocalizedString Description => S["Uploads files to a folder of an FTP server."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-server";

    /// <summary>
    /// Describes where a step uploads its files, such as <c>ftps://host:21/folder</c>. It holds no secrets.
    /// </summary>
    /// <param name="settings">The settings of the step.</param>
    /// <returns>The description.</returns>
    public static string GetSummary(UploadToFtpStepSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            return null;
        }

        return DataPipelineRemotePaths.ToDisplayUrl(GetScheme(settings.Encryption), settings.Host.Trim(), settings.Port, settings.RemoteFolder?.Trim());
    }

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            context.AddError(S["Enter the host of the FTP server."]);
        }
        else if (Uri.CheckHostName(settings.Host.Trim()) == UriHostNameType.Unknown)
        {
            context.AddError(S["The host '{0}' isn't a valid host name or IP address.", settings.Host]);
        }

        if (settings.Port is < 1 or > 65535)
        {
            context.AddError(S["The port must be between 1 and 65535."]);
        }

        if (string.IsNullOrWhiteSpace(settings.Username))
        {
            context.AddError(S["Enter the user name, such as anonymous for a public server."]);
        }

        if (!string.IsNullOrEmpty(settings.ProtectedPassword) && _secrets.Unprotect(settings.ProtectedPassword) is null)
        {
            context.AddError(S["The password can't be read on this site, such as when its data protection keys changed: enter it again."]);
        }

        if (settings.TimeoutSeconds < 1)
        {
            context.AddError(S["The timeout must be at least one second."]);
        }

        if (settings.Encryption == DataPipelineFtpEncryption.None)
        {
            context.AddWarning(S["The connection isn't encrypted: the credentials and the files travel in clear text."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var host = settings.Host?.Trim();
        var password = _secrets.Unprotect(settings.ProtectedPassword);
        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var folder = DataPipelineRemotePaths.RenderFolder(settings.RemoteFolder, context.Run, now);
        var scheme = GetScheme(settings.Encryption);

        var connection = new DataPipelineFtpConnection
        {
            Host = host,
            Port = settings.Port,
            Encryption = settings.Encryption,
            ValidateCertificate = settings.ValidateCertificate,
            Username = settings.Username?.Trim(),
            Password = password,
            Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds)),
        };

        try
        {
            await DataPipelineRemotePaths.UploadAsync(
                context,
                () => _clientFactory.CreateClient(connection),
                () =>
                {
                    if (settings.Encryption == DataPipelineFtpEncryption.None)
                    {
                        context.LogWarning(S["The connection to the FTP server '{0}' isn't encrypted: the credentials and the files travel in clear text.", host]);
                    }
                    else if (!settings.ValidateCertificate)
                    {
                        context.LogWarning(S["The certificate of the FTP server '{0}' isn't validated.", host]);
                    }
                },
                folder,
                settings.Overwrite,
                (fileName, remotePath) => S["Uploaded '{0}' to {1}", fileName, DataPipelineRemotePaths.ToDisplayUrl(scheme, host, settings.Port, remotePath)],
                remotePath => S["The file '{0}' already exists on the FTP server '{1}', and the step doesn't overwrite files.", remotePath, host]);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not DataPipelineDestinationException)
        {
            throw new DataPipelineDestinationException(
                S["Could not upload to the FTP server '{0}': {1}", host, DataPipelineRemotePaths.Redact(ex.Message, password)],
                ex);
        }
    }

    private static string GetScheme(DataPipelineFtpEncryption encryption)
        => encryption == DataPipelineFtpEncryption.None ? "ftp" : "ftps";
}
