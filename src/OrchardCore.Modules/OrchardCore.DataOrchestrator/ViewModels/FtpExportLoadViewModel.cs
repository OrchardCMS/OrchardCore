namespace OrchardCore.DataOrchestrator.ViewModels;

public sealed class FtpExportLoadViewModel : FileExportLoadViewModel
{
    public string Host { get; set; }

    public int Port { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string RemoteDirectory { get; set; }

    public string SecurityMode { get; set; }

    public bool AcceptAnyCertificate { get; set; }

    public bool Passive { get; set; }
}
