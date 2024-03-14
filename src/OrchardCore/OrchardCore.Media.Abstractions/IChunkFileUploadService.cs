using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Media;

/// <summary>
/// Represents a service that manages chunked file uploads.
/// </summary>
public interface IChunkFileUploadService
{
    /// <summary>
    /// Processes the <paramref name="request"/> and handles chunked or regular file uploads depending on the
    /// <see cref="MediaOptions"/> and <paramref name="request"/> content.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="chunkAsync">Callback to report the consumer and create the <see cref="IActionResult"/> in case of
    /// a chunk was uploaded.</param>
    /// <param name="completedAsync">Callback to report the consumer and create the <see cref="IActionResult"/> in case
    /// of all chunks were uploaded or a regular file upload happened.</param>
    Task<IActionResult> ProcessRequestAsync(
        HttpRequest request,
        Func<Guid, IFormFile, ContentRangeHeaderValue, Task<IActionResult>> chunkAsync,
        Func<IEnumerable<IFormFile>, Task<IActionResult>> completedAsync);

    /// <summary>
    /// Purges temporary files left by abandoned uploads from the temporary directory.
    /// </summary>
    void PurgeTempDirectory();
}
