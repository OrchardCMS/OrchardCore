#nullable enable

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Models;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Middleware;

internal sealed class MediaImageProcessingMiddleware : IMiddleware
{
    private static readonly HashSet<string> _imageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
    };

    private readonly IMediaFileProvider _mediaFileProvider;
    private readonly IImageProcessingEngine _engine;
    private readonly IResizedImageCache _cache;
    private readonly MediaCommandParser _commandParser;
    private readonly SingleFlight<string, string> _singleFlight;
    private readonly IOptions<MediaOptions> _mediaOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public MediaImageProcessingMiddleware(
        IMediaFileProvider mediaFileProvider,
        IImageProcessingEngine engine,
        IResizedImageCache cache,
        MediaCommandParser commandParser,
        SingleFlight<string, string> singleFlight,
        IOptions<MediaOptions> mediaOptions,
        ShellSettings shellSettings,
        ILogger<MediaImageProcessingMiddleware> logger)
    {
        _mediaFileProvider = mediaFileProvider;
        _engine = engine;
        _cache = cache;
        _commandParser = commandParser;
        _singleFlight = singleFlight;
        _mediaOptions = mediaOptions;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestPath = context.Request.Path;

        // Only handle requests under the media assets path. This is the common case for most
        // requests, so the cheap checks are kept in this non-async method and the request delegate
        // is returned directly to avoid allocating an async state machine for non-image requests.
        if (!requestPath.StartsWithSegments(_mediaOptions.Value.AssetsRequestPath, StringComparison.OrdinalIgnoreCase, out var remaining))
        {
            return next(context);
        }

        // Only handle image file requests.
        var ext = Path.GetExtension(remaining.Value);
        if (string.IsNullOrEmpty(ext) || !_imageExtensions.Contains(ext))
        {
            return next(context);
        }

        return ProcessRequestAsync(context, next, remaining, ext);
    }

    private async Task ProcessRequestAsync(HttpContext context, RequestDelegate next, PathString remaining, string ext)
    {
        var mediaOptions = _mediaOptions.Value;

        // Parse and validate resize commands from the query string.
        var commands = _commandParser.Parse(context);
        if (commands is null)
        {
            await next(context);
            return;
        }

        // If the format is not explicitly requested, default to the source file's format.
        if (string.IsNullOrEmpty(commands.Format))
        {
            commands.Format = MediaResizingConstants.ExtensionToFormat(ext);
        }

        // The output format is fully determined here, so the cache file extension and content type
        // are known before the cache lookup.
        var format = MediaResizingConstants.ParseFormat(commands.Format);
        var contentType = MediaResizingConstants.FormatToContentType(format);
        var fileExtension = MediaResizingConstants.ContentTypeToExtension(contentType);

        // Include the optional version token ("v") in the cache key so that replacing the source
        // file (which changes the version) produces a new cache entry instead of serving a stale
        // resized image.
        var keyCommands = commands.GetValues();
        var versionValue = context.Request.Query[MediaCommands.VersionCommand].ToString();
        if (!string.IsNullOrEmpty(versionValue))
        {
            keyCommands = keyCommands.Append(new KeyValuePair<string, string>(MediaCommands.VersionCommand, versionValue));
        }

        var cacheKey = ResizedImageCacheKey.Compute(
            _shellSettings.Name,
            remaining.Value!,
            keyCommands);

        // Try the cache first.
        var cached = await _cache.GetAsync(cacheKey, fileExtension, context.RequestAborted);
        if (cached.HasValue)
        {
            // ServeAsync owns and disposes the cached content stream.
            await ServeAsync(context, cached.Value.Content, cached.Value.ContentType, mediaOptions);
            return;
        }

        // Read the source file.
        var fileInfo = _mediaFileProvider.GetFileInfo(remaining.Value!);
        if (!fileInfo.Exists)
        {
            await next(context);
            return;
        }

        // Coalesce concurrent cold-cache requests for the same key so the expensive transform runs
        // only once. The single worker processes the image and writes it to the cache; every waiter
        // (including the worker) then serves its own independent stream read back from the cache.
        // The factory returns the produced content type, or null when the source file is missing or
        // processing failed. CancellationToken.None is used for the shared work so that one caller
        // aborting its request does not fail the operation for the other waiters.
        var produced = await _singleFlight.ScheduleAsync(cacheKey, async _ =>
        {
            // Another worker may have populated the cache after our initial probe but before we
            // acquired the single-flight slot; re-check so we do not reprocess needlessly.
            var existing = await _cache.GetAsync(cacheKey, fileExtension, CancellationToken.None);
            if (existing.HasValue)
            {
                existing.Value.Content.Dispose();
                return existing.Value.ContentType;
            }

            ImageProcessingResult result;
            try
            {
                var engineCommands = BuildEngineCommands(commands, format);
                using var source = fileInfo.CreateReadStream();
                result = await _engine.ProcessAsync(source, engineCommands, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image '{Path}'.", remaining.Value);
                return null;
            }

            using (result)
            {
                result.Output.Position = 0;
                await _cache.SetAsync(
                    cacheKey,
                    result.Output,
                    result.ContentType,
                    TimeSpan.FromDays(mediaOptions.MaxCacheDays),
                    CancellationToken.None);
            }

            return result.ContentType;
        });

        if (produced is null)
        {
            // The source file disappeared or processing failed.
            await next(context);
            return;
        }

        // Serve from the cache the single worker populated.
        var entry = await _cache.GetAsync(cacheKey, fileExtension, context.RequestAborted);
        if (!entry.HasValue)
        {
            // The entry was evicted between being written and read back (very rare).
            _logger.LogWarning("Resized image cache entry for '{Path}' was missing immediately after processing.", remaining.Value);
            await next(context);
            return;
        }

        // ServeAsync owns and disposes the cached content stream.
        await ServeAsync(context, entry.Value.Content, entry.Value.ContentType, mediaOptions);
    }

    private static async Task ServeAsync(HttpContext context, Stream content, string contentType, MediaOptions options)
    {
        // Take ownership of the content stream so it is always released, including on the cache-hit
        // hot path where the stream is a physical FileStream or a remote network stream.
        await using (content)
        {
            var response = context.Response;
            response.ContentType = contentType;

            // Apply cache-control header.
            var cacheControl = context.IsSecureMediaRequested()
                ? options.MaxSecureFilesBrowserCacheDays == 0
                    ? "no-store"
                    : $"public, must-revalidate, max-age={TimeSpan.FromDays(options.MaxSecureFilesBrowserCacheDays).TotalSeconds}"
                : $"public, max-age={TimeSpan.FromDays(options.MaxBrowserCacheDays).TotalSeconds}";

            response.Headers.CacheControl = cacheControl;

            if (content.CanSeek)
            {
                response.ContentLength = content.Length - content.Position;
            }

            await content.CopyToAsync(response.Body, context.RequestAborted);
        }
    } 

    // Maps the validated, string-based request commands to the engine-agnostic, typed command set
    // consumed by IImageProcessingEngine. Parsing happens once here so engines never touch raw
    // query values.
    private static ImageProcessingCommands BuildEngineCommands(MediaCommands commands, Format format)
    {
        var engineCommands = new ImageProcessingCommands
        {
            Format = format,
            BackgroundColor = string.IsNullOrEmpty(commands.BackgroundColor) ? null : commands.BackgroundColor,
            AutoOrient = !string.Equals(commands.AutoOrient, "false", StringComparison.OrdinalIgnoreCase),
        };

        if (int.TryParse(commands.Width, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) && width > 0)
        {
            engineCommands.Width = width;
        }

        if (int.TryParse(commands.Height, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) && height > 0)
        {
            engineCommands.Height = height;
        }

        if (int.TryParse(commands.Quality, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quality))
        {
            engineCommands.Quality = quality;
        }

        if (Enum.TryParse<ResizeMode>(commands.ResizeMode, ignoreCase: true, out var mode))
        {
            engineCommands.ResizeMode = mode;
        }

        var (focalX, focalY) = ParseFocalPoint(commands.ResizeFocalPoint);
        engineCommands.FocalPointX = focalX;
        engineCommands.FocalPointY = focalY;

        return engineCommands;
    }

    private static (float? X, float? Y) ParseFocalPoint(string? focalPoint)
    {
        if (string.IsNullOrEmpty(focalPoint))
        {
            return (null, null);
        }

        var comma = focalPoint.IndexOf(',');
        if (comma < 1)
        {
            return (null, null);
        }

        if (float.TryParse(focalPoint.AsSpan(0, comma), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            float.TryParse(focalPoint.AsSpan(comma + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return (Math.Clamp(x, 0f, 1f), Math.Clamp(y, 0f, 1f));
        }

        return (null, null);
    }
}
