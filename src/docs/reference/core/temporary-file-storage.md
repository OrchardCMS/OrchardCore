# Temporary File Storage

When Orchard Core needs to write a file temporarily — an in-progress chunked or resumable (TUS) upload, an extracted deployment/recipe archive, or a recipe written to disk before setup — it goes through the `ITempDirectoryProvider` abstraction (`OrchardCore.FileStorage`) instead of writing directly to the operating system temporary directory.

The default implementation, `DefaultTempDirectoryProvider`, is filesystem based. It stores everything under a **tenant-scoped** sub-directory of a configurable base path, so operators can relocate temporary storage onto a larger or shared volume, and every tenant's temporary files stay separated from one another.

## Why this exists

Previously these consumers wrote to `Path.GetTempPath()`, whose available space is often limited (small system temp volumes, ephemeral container disks). Under heavy concurrent uploads of large files, that volume can fill up. Making the base path configurable lets you point temporary storage at a volume sized for the workload — including a shared network volume in multi-instance deployments, where an upload started on one instance can be completed on another.

## Configuration

The base location is controlled globally by the `OrchardCore:TempDirectory` configuration section:

```json
{
  "OrchardCore": {
    "TempDirectory": {
      // Base path under which tenant-scoped temporary files are stored.
      // When omitted, the operating system temporary directory (Path.GetTempPath()) is used.
      "Path": "/mnt/shared/temp"
    }
  }
}
```

Files are laid out as `{Path}/{TenantName}/...`. The tenant sub-directory is added automatically; you never compose it yourself. When `Path` is not set, the store falls back to the operating system temporary directory, preserving the previous behavior.

This one setting applies to every temporary file consumer: media chunked uploads, TUS uploads, deployment and recipe imports/exports, and tenant recipe uploads.

!!! note
    Resumable (TUS) partial uploads are stored in a `TusUploads` sub-directory of this location, so they follow `OrchardCore:TempDirectory:Path` like every other temporary file.

## Using a mounted file share (Azure Files, AWS EFS/FSx)

Because `DefaultTempDirectoryProvider` uses plain `System.IO` operations, **any storage that presents itself as a mounted filesystem path works with no code changes** — local disk, a SAN/NAS volume, an SMB share (Azure Files), or an NFS share (AWS EFS/FSx). Mount it at the operating system level, then set `Path` to the mount point.

=== "Linux / containers (SMB)"

    ```bash
    sudo mkdir -p /mnt/octemp
    sudo mount -t cifs //<account>.file.core.windows.net/<share> /mnt/octemp \
      -o vers=3.0,username=<account>,password=<storage-key>,dir_mode=0777,file_mode=0777,serverino,nosharesock,actimeo=30
    ```

    ```json
    { "OrchardCore": { "TempDirectory": { "Path": "/mnt/octemp" } } }
    ```

=== "Windows"

    ```powershell
    New-SmbGlobalMapping -RemotePath "\\<account>.file.core.windows.net\<share>" -Credential $cred -Persistent $true -LocalPath Z:
    ```

    ```json
    { "OrchardCore": { "TempDirectory": { "Path": "Z:\\octemp" } } }
    ```

=== "Azure App Service / Container Apps"

    Use the platform's Azure Files mount (App Service: *Configuration → Path mappings → Azure Storage Mounts*; Container Apps: an `AzureFile` volume). Then set `Path` to the platform mount path (e.g. `/mounts/octemp`). The platform manages the credentials and reconnection.

!!! warning
    Point `Path` at a mounted **file share**, not object storage. The temporary file consumers require real, seekable local files — for example the chunked-upload path does `Seek`/`SetLength` and reopens the same file across requests, and deployment import uses `ZipFile.ExtractToDirectory` and `PhysicalFileProvider`. Azure Files (SMB) and AWS EFS/FSx support this; Azure Blob and AWS S3 object storage do not. Resumable (TUS) uploads are the exception: they can target object storage through the `ITusTempStore` implementations in the `OrchardCore.Media.Azure` and `OrchardCore.Media.AmazonS3` features.

Operational notes: make sure the mount is writable by the identity the application runs as; keep the share in the same region to limit latency; and note that existing cleanup (such as the media `TemporaryFileLifetime` purge) now runs against the configured path.

## Consuming `ITempDirectoryProvider`

If your feature writes temporary files, inject `ITempDirectoryProvider` instead of calling `Path.GetTempPath()`. This keeps your temporary files under the configured, tenant-scoped location automatically.

| Member | Use it for |
| --- | --- |
| `GetRootDirectory()` | The tenant-scoped root directory (created on demand). Use as the base for tools that need a directory, such as a `TemporaryFileBuilder`. |
| `GetOrCreateSubdirectory(name)` | A named, reusable sub-directory (e.g. a per-feature bucket). Path traversal outside the root is rejected. |
| `GetTempFileName(extension)` | A unique file path (not created) for writing a single temporary file. Optional extension, with or without a leading dot. |
| `CreateTempSubdirectory()` | A newly created, unique sub-directory — for extracting an archive into, or exposing through a `PhysicalFileProvider`. |

```csharp
using OrchardCore.FileStorage;

public sealed class MyImportService
{
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public MyImportService(ITempDirectoryProvider tempDirectoryProvider)
        => _tempDirectoryProvider = tempDirectoryProvider;

    public async Task ImportAsync(IFormFile package, CancellationToken cancellationToken)
    {
        // A unique file path under {Path}/{TenantName}/ to save the upload.
        var archivePath = _tempDirectoryProvider.GetTempFileName(Path.GetExtension(package.FileName));

        // A dedicated directory to extract into.
        var extractPath = _tempDirectoryProvider.CreateTempSubdirectory();

        try
        {
            await using (var stream = File.Create(archivePath))
            {
                await package.CopyToAsync(stream, cancellationToken);
            }

            ZipFile.ExtractToDirectory(archivePath, extractPath);

            // ... process the extracted files ...
        }
        finally
        {
            File.Delete(archivePath);
            Directory.Delete(extractPath, recursive: true);
        }
    }
}
```

The store hands out paths only; you remain responsible for creating, writing, and cleaning up the files themselves. Delete temporary files when you are done with them — the store does not track or expire them on your behalf.

!!! warning
    `ITempDirectoryProvider` is for *temporary* storage, and it does not scan uploads. When the temporary file comes from a user upload, still run it through [`FileCreationService`](file-upload-security.md) before you store it permanently.

## Replacing the implementation

`DefaultTempDirectoryProvider` is registered per tenant with `TryAddSingleton`, so a module can substitute its own implementation:

```csharp
services.Replace(ServiceDescriptor.Singleton<ITempDirectoryProvider, MyTempDirectoryProvider>());
```

Any replacement must honor the same contract: return **real local filesystem paths** that support random access and directory enumeration. This is why there is no Azure Blob or AWS S3 implementation of `ITempDirectoryProvider` — those object stores cannot satisfy the path-based, seekable contract the consumers depend on. To move temporary storage to the cloud, mount a file share (Azure Files, AWS EFS/FSx) and configure `Path`, as described above.
