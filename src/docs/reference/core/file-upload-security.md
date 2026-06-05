# File Upload Security

When your feature accepts a user-uploaded file and stores it outside Orchard Core's built-in media flow, use `FileCreationService` to run the shared pre-storage security pipeline.

This ensures every `IFileEventHandler` can inspect or replace the stream before the file is written permanently. If any handler returns a failed `FileCreatingResult`, the upload must be aborted and the file must not be stored.

## When to use `FileCreationService`

Use `FileCreationService` in custom controllers, admin endpoints, APIs, recipe importers, or background flows that:

- accept a file from a user;
- write that file to disk, cloud storage, or another permanent store; and
- do **not** already go through `IMediaFileStore.CreateFileFromStreamAsync()`.

!!! note
    `DefaultMediaFileStore` already uses `FileCreationService` internally. If your code uploads through `IMediaFileStore`, do not call the service a second time.

## Core types

- `FileCreationService`: runs `CreatingAsync()` before storage and `CreatedAsync()` after storage succeeds.
- `FileCreatingContext`: describes the file being processed.
- `FileCreatingResult`: returns the stream that should continue through the pipeline and whether creation should proceed.
- `IFileEventHandler`: participates in the upload pipeline.

## Ownership and cleanup

- The original upload stream stays owned by the caller.
- If a handler replaces the stream, `FileCreationService` disposes the superseded intermediate stream.
- The `FileCreatingResult` returned from `CreateAsync()` owns the final replacement stream when one was created, so callers should use `await using` and keep that result alive for as long as they need the processed stream.
- If `CreateAsync()` returns a failed result, dispose that result the same way and abort the upload without storing the file.
- After the file has been written permanently, disposing `FileCreatingResult` cleans up any temporary replacement stream used during the upload pipeline.

## Using `FileCreationService`

Call `CreateAsync()` before writing the file. If the returned result did not succeed, abort the request. Only call `CreatedAsync()` after the file was stored successfully.

```csharp
using Microsoft.AspNetCore.Http;
using OrchardCore.FileStorage;

public sealed class CustomUploadService
{
    private readonly FileCreationService _fileCreationService;
    private readonly IFileStore _fileStore;

    public CustomUploadService(
        FileCreationService fileCreationService,
        IFileStore fileStore)
    {
        _fileCreationService = fileCreationService;
        _fileStore = fileStore;
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var uploadedStream = file.OpenReadStream();
        await using var fileCreatingResult = await _fileCreationService.CreateAsync(
            new FileCreatingContext(file.FileName, file.Length, file.ContentType),
            uploadedStream,
            cancellationToken);

        if (!fileCreatingResult.Succeeded)
        {
            throw new FileStoreException(fileCreatingResult.ErrorMessage ?? $"The uploaded file '{file.FileName}' was rejected before it could be stored.");
        }

        // Use the processed stream while the result is still in scope.
        var path = await _fileStore.CreateFileFromStreamAsync(file.FileName, fileCreatingResult.Stream);
        var fileInfo = await _fileStore.GetFileInfoAsync(path);

        await _fileCreationService.CreatedAsync(fileInfo, cancellationToken);

        return path;
    }
}
```

## Implementing a handler

Return `FileCreatingResult.Failed(...)` from `IFileEventHandler.CreatingAsync()` to stop the upload before it is stored. This is where a module would perform checks such as antivirus scanning, content inspection, or file-type validation.

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure;

namespace MyModule.Services;

public sealed class RejectExecutableFileEventHandler : IFileEventHandler
{
    public Task<FileCreatingResult> CreatingAsync(FileCreatingContext context, Stream stream, CancellationToken cancellationToken = default)
    {
        if (string.Equals(Path.GetExtension(context.FileName), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(FileCreatingResult.Failed(stream, new ResultError
            {
                Message = new LocalizedString(nameof(RejectExecutableFileEventHandler), "Executable files are not allowed."),
            }));
        }

        return Task.FromResult(FileCreatingResult.Success(stream));
    }

    public Task CreatedAsync(IFileStoreEntry fileInfo, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

## Security guidance

Always run `FileCreationService` before any permanent write. Do not save the uploaded file first and scan it afterward, since a failed scan must abort the upload before the file is persisted.
