using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Uploads the files it reads to a folder of an SFTP server, authenticating with a password, a private key, or both,
/// and verifying the host key of the server when its fingerprint is set.
/// </summary>
public sealed class UploadToSftpStep : DataPipelineStepType<UploadToSftpStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "UploadToSftp";

    private readonly IDataPipelineSftpClientFactory _clientFactory;
    private readonly DataPipelineSecrets _secrets;
    private readonly IStringLocalizer S;

    public UploadToSftpStep(
        IDataPipelineSftpClientFactory clientFactory,
        DataPipelineSecrets secrets,
        IStringLocalizer<UploadToSftpStep> localizer)
    {
        _clientFactory = clientFactory;
        _secrets = secrets;
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Upload to an SFTP server"];

    public override LocalizedString Description => S["Uploads files to a folder of an SFTP server, over SSH."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-lock";

    /// <summary>
    /// Describes where a step uploads its files, such as <c>sftp://host:22/folder</c>. It holds no secrets.
    /// </summary>
    /// <param name="settings">The settings of the step.</param>
    /// <returns>The description.</returns>
    public static string GetSummary(UploadToSftpStepSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            return null;
        }

        return DataPipelineRemotePaths.ToDisplayUrl("sftp", settings.Host.Trim(), settings.Port, settings.RemoteFolder?.Trim());
    }

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            context.AddError(S["Enter the host of the SFTP server."]);
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
            context.AddError(S["Enter the user name."]);
        }

        if (string.IsNullOrEmpty(settings.ProtectedPassword) && string.IsNullOrEmpty(settings.ProtectedPrivateKey))
        {
            context.AddError(S["Enter a password or a private key."]);
        }

        if ((!string.IsNullOrEmpty(settings.ProtectedPassword) && _secrets.Unprotect(settings.ProtectedPassword) is null) ||
            (!string.IsNullOrEmpty(settings.ProtectedPrivateKey) && _secrets.Unprotect(settings.ProtectedPrivateKey) is null) ||
            (!string.IsNullOrEmpty(settings.ProtectedPassphrase) && _secrets.Unprotect(settings.ProtectedPassphrase) is null))
        {
            context.AddError(S["The credentials can't be read on this site, such as when its data protection keys changed: enter them again."]);
        }

        if (string.IsNullOrWhiteSpace(settings.HostKeyFingerprint))
        {
            context.AddWarning(S["The host key of the server isn't verified: enter its SHA-256 fingerprint to make sure the files go to the right server."]);
        }
        else if (!DataPipelineHostKeyFingerprint.IsValid(settings.HostKeyFingerprint))
        {
            context.AddError(S["The host key fingerprint must be a SHA-256 fingerprint, such as SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og."]);
        }

        if (settings.TimeoutSeconds < 1)
        {
            context.AddError(S["The timeout must be at least one second."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var host = settings.Host?.Trim();
        var password = _secrets.Unprotect(settings.ProtectedPassword);
        var privateKey = _secrets.Unprotect(settings.ProtectedPrivateKey);
        var passphrase = _secrets.Unprotect(settings.ProtectedPassphrase);
        var expected = string.IsNullOrWhiteSpace(settings.HostKeyFingerprint) ? null : settings.HostKeyFingerprint;
        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var folder = DataPipelineRemotePaths.RenderFolder(settings.RemoteFolder, context.Run, now);
        string presented = null;

        var connection = new DataPipelineSftpConnection
        {
            Host = host,
            Port = settings.Port,
            Username = settings.Username?.Trim(),
            Password = password,
            PrivateKey = privateKey,
            Passphrase = passphrase,
            Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds)),
            ValidateHostKey = fingerprint =>
            {
                presented = DataPipelineHostKeyFingerprint.Normalize(fingerprint) ?? fingerprint;

                return expected is null || DataPipelineHostKeyFingerprint.Matches(expected, fingerprint);
            },
        };

        try
        {
            await DataPipelineRemotePaths.UploadAsync(
                context,
                () => _clientFactory.CreateClient(connection),
                () =>
                {
                    if (expected is null)
                    {
                        context.LogWarning(S["The host key of the SFTP server '{0}' isn't verified. To verify it, set its fingerprint: SHA256:{1}", host, presented]);
                    }
                },
                folder,
                settings.Overwrite,
                (fileName, remotePath) => S["Uploaded '{0}' to {1}", fileName, DataPipelineRemotePaths.ToDisplayUrl("sftp", host, settings.Port, remotePath)],
                remotePath => S["The file '{0}' already exists on the SFTP server '{1}', and the step doesn't overwrite files.", remotePath, host]);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not DataPipelineDestinationException)
        {
            if (expected is not null && presented is not null && !DataPipelineHostKeyFingerprint.Matches(expected, presented))
            {
                throw new DataPipelineDestinationException(
                    S["The SFTP server '{0}' presented the host key SHA256:{1}, which doesn't match the expected fingerprint. The connection was refused.", host, presented],
                    ex);
            }

            throw new DataPipelineDestinationException(
                S["Could not upload to the SFTP server '{0}': {1}", host, DataPipelineRemotePaths.Redact(ex.Message, password, privateKey, passphrase)],
                ex);
        }
    }
}
