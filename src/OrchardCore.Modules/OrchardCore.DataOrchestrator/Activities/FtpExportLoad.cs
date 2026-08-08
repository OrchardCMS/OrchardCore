using Microsoft.AspNetCore.DataProtection;
using FluentFTP;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Exports the pipeline data to an FTP or FTPS server using the selected format.
/// This activity also serves as a reference implementation for building custom, extensible
/// destinations: deriving from <see cref="EtlFileExportLoad"/> and overriding
/// <see cref="EtlFileExportLoad.WriteToDestinationAsync"/> is all that is required to add a new
/// destination (for example a cloud bucket or a reporting server).
/// </summary>
public sealed class FtpExportLoad : EtlFileExportLoad
{
    private const string ProtectedPasswordPrefix = "protected:";
    private const string PasswordProtectorPurpose = "OrchardCore.DataOrchestrator.FtpExportLoad.Password";

    private readonly IDataProtector _dataProtector;

    public const string SecurityNone = "None";
    public const string SecurityExplicit = "Explicit";
    public const string SecurityImplicit = "Implicit";
    public const string SecurityAuto = "Auto";

    public FtpExportLoad(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector(PasswordProtectorPurpose);
    }

    public override string Name => nameof(FtpExportLoad);

    public override string DisplayText => "FTP / FTPS Export";

    /// <summary>
    /// Gets or sets the FTP server host name or IP address.
    /// </summary>
    public string Host
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the FTP server port. Defaults to 21.
    /// </summary>
    public int Port
    {
        get => GetProperty(() => 21);
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the user name used to authenticate.
    /// </summary>
    public string Username
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the password used to authenticate.
    /// </summary>
    public string Password
    {
        get
        {
            var protectedPassword = GetProperty<string>();

            if (string.IsNullOrEmpty(protectedPassword))
            {
                return null;
            }

            if (!protectedPassword.StartsWith(ProtectedPasswordPrefix, StringComparison.Ordinal))
            {
                return protectedPassword;
            }

            return _dataProtector.Unprotect(protectedPassword[ProtectedPasswordPrefix.Length..]);
        }

        set => SetProperty(string.IsNullOrEmpty(value) ? null : ProtectedPasswordPrefix + _dataProtector.Protect(value));
    }

    /// <summary>
    /// Gets or sets the remote directory the file is uploaded to. The directory is created when missing.
    /// </summary>
    public string RemoteDirectory
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the transport security mode: <c>None</c>, <c>Explicit</c>, <c>Implicit</c>, or <c>Auto</c>.
    /// </summary>
    public string SecurityMode
    {
        get => GetProperty(() => SecurityAuto);
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets whether to accept any TLS certificate, including self-signed ones.
    /// </summary>
    public bool AcceptAnyCertificate
    {
        get => GetProperty(() => false);
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets whether to use passive mode for the data connection. Passive mode is recommended
    /// for clients behind a firewall or NAT. Defaults to <c>true</c>.
    /// </summary>
    public bool Passive
    {
        get => GetProperty(() => true);
        set => SetProperty(value);
    }

    protected override async Task WriteToDestinationAsync(EtlExecutionContext context, string fileName, Stream content, IEtlExportFormat format)
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("The FTP host is required.");
        }

        var config = new FtpConfig
        {
            EncryptionMode = ResolveEncryptionMode(),
            ValidateAnyCertificate = AcceptAnyCertificate,
            DataConnectionType = Passive ? FtpDataConnectionType.AutoPassive : FtpDataConnectionType.AutoActive,
        };

        var port = Port > 0 ? Port : 21;

        await using var client = new AsyncFtpClient(Host, Username ?? string.Empty, Password ?? string.Empty, port, config, null);

        await client.Connect(context.CancellationToken);

        try
        {
            var remotePath = CombineRemotePath(RemoteDirectory, fileName);

            await client.UploadStream(content, remotePath, FtpRemoteExists.Overwrite, true, null, context.CancellationToken);
        }
        finally
        {
            await client.Disconnect(context.CancellationToken);
        }
    }

    private FtpEncryptionMode ResolveEncryptionMode()
    {
        return SecurityMode switch
        {
            SecurityNone => FtpEncryptionMode.None,
            SecurityExplicit => FtpEncryptionMode.Explicit,
            SecurityImplicit => FtpEncryptionMode.Implicit,
            _ => FtpEncryptionMode.Auto,
        };
    }

    internal static string CombineRemotePath(string directory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return fileName;
        }

        return $"{directory.TrimEnd('/')}/{fileName.TrimStart('/')}";
    }
}
