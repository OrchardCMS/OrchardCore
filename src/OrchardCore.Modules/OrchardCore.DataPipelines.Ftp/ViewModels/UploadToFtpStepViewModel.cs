using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Ftp.ViewModels;

/// <summary>
/// The view model of the editor and of the summary of the <see cref="UploadToFtpStep"/>.
/// It is not sealed, as its shapes are proxies that derive from it.
/// </summary>
public class UploadToFtpStepViewModel
{
    public string Host { get; set; }

    public int Port { get; set; }

    public DataPipelineFtpEncryption Encryption { get; set; }

    public bool ValidateCertificate { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool RemovePassword { get; set; }

    public string RemoteFolder { get; set; }

    public bool Overwrite { get; set; }

    public int TimeoutSeconds { get; set; }

    [BindNever]
    public bool HasPassword { get; set; }

    [BindNever]
    public string Summary { get; set; }
}
