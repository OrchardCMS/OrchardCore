namespace OrchardCore.FileStorage;

public interface IAntivirusScanner
{
    Task<AntivirusResult> ScanAsync(AntivirusScanContext context, Stream stream);
}
