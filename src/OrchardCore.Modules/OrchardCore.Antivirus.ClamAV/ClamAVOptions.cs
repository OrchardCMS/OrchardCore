namespace OrchardCore.Antivirus.ClamAV;

public sealed class ClamAvOptions
{
    public const string ConfigSection = "OrchardCore_Antivirus_ClamAV";

    public string Host { get; set; }

    public int Port { get; set; } = 3310;

    public int ConnectTimeoutSeconds { get; set; } = 5;

    public int TransferTimeoutSeconds { get; set; } = 30;
}
