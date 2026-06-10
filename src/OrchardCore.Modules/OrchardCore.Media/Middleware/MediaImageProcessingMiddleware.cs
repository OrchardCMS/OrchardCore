#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
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
    private readonly IOptions<MediaOptions> _mediaOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public MediaImageProcessingMiddleware(
        IMediaFileProvider mediaFileProvider,
        IImageProcessingEngine engine,
        IResizedImageCache cache,
        MediaCommandParser commandParser,
        IOptions<MediaOptions> mediaOptions,
        ShellSettings shellSettings,
        ILogger<MediaImageProcessingMiddleware> logger)
    {
        _mediaFileProvider = mediaFileProvider;
        _engine = engine;
        _cache = cache;
        _commandParser = commandParser;
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
            commands.Format = ExtensionToFormat(ext);
        }

        // The output format is fully determined here, so the cache file extension and content type
        // are known before the cache lookup.
        var fileExtension = MediaResizingConstants.ContentTypeToExtension(FormatToContentType(commands.Format));

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

        // Process the image.
        ImageProcessingResult result;
        try
        {
            using var source = fileInfo.CreateReadStream();
            result = await _engine.ProcessAsync(source, commands, context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image '{Path}'.", remaining.Value);
            await next(context);
            return;
        }

        using (result)
        {
            // Store in cache.
            result.Output.Position = 0;
            await _cache.SetAsync(
                cacheKey,
                result.Output,
                result.ContentType,
                TimeSpan.FromDays(mediaOptions.MaxCacheDays),
                context.RequestAborted);

            result.Output.Position = 0;

            // ServeAsync disposes the stream; the surrounding `using` disposing the result again
            // is a harmless no-op on the already-disposed MemoryStream.
            await ServeAsync(context, result.Output, result.ContentType, mediaOptions);
        }
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

    private static string ExtensionToFormat(string ext) => ext.TrimStart('.').ToLowerInvariant() switch
    {
        "jpeg" => "jpg",
        "png"  => "png",
        "gif"  => "gif",
        "webp" => "webp",
        _      => "jpg",
    };

    private static string FormatToContentType(string? format) => format?.ToLowerInvariant() switch
    {
        "png"  => MediaResizingConstants.PngContentType,
        "gif"  => MediaResizingConstants.GifContentType,
        "webp" => MediaResizingConstants.WebpContentType,
        _      => MediaResizingConstants.JpegContentType,
    };
}
