using Microsoft.Extensions.Options;

namespace OrchardCore.DataProtection.Azure;

public class BlobOptions : IAsyncOptions
{
    public string ConnectionString { get; set; }

    public string ContainerName { get; set; } = "dataprotection";

    public string BlobName { get; set; }

    public bool CreateContainer { get; set; } = true;
}
