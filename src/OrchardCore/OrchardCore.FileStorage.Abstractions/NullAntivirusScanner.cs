namespace OrchardCore.FileStorage;

public sealed class NullAntivirusScanner : IAntivirusScanner
{
    public Task<AntivirusResult> ScanAsync(AntivirusScanContext context, Stream stream)
        => Task.FromResult(AntivirusResult.Clean);
}
