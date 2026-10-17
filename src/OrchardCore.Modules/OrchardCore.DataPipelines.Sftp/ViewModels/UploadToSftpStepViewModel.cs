using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Sftp.ViewModels;

/// <summary>
/// The view model of the editor and of the summary of the <see cref="UploadToSftpStep"/>.
/// It is not sealed, as its shapes are proxies that derive from it.
/// </summary>
public class UploadToSftpStepViewModel
{
    public string Host { get; set; }

    public int Port { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool RemovePassword { get; set; }

    public string PrivateKey { get; set; }

    public bool RemovePrivateKey { get; set; }

    public string Passphrase { get; set; }

    public bool RemovePassphrase { get; set; }

    public string HostKeyFingerprint { get; set; }

    public string RemoteFolder { get; set; }

    public bool Overwrite { get; set; }

    public int TimeoutSeconds { get; set; }

    [BindNever]
    public bool HasPassword { get; set; }

    [BindNever]
    public bool HasPrivateKey { get; set; }

    [BindNever]
    public bool HasPassphrase { get; set; }

    [BindNever]
    public string Summary { get; set; }
}
