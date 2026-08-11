using System.Text.RegularExpressions;

namespace OrchardCore.FileStorage.AzureBlob;

/// <summary>
/// Validates Azure Blob container names against the rules documented at
/// https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata.
/// </summary>
public static partial class BlobContainerNameValidator
{
    // 3-63 characters, starts and ends with lowercase letter or digit,
    // contains only lowercase letters, digits and single hyphens (no consecutive hyphens).
    [GeneratedRegex("^[a-z0-9](?:[a-z0-9]|-(?=[a-z0-9])){2,62}$")]
    private static partial Regex ContainerNameRegex();

    public static bool IsValid(string containerName)
        => !string.IsNullOrEmpty(containerName) && ContainerNameRegex().IsMatch(containerName);
}
