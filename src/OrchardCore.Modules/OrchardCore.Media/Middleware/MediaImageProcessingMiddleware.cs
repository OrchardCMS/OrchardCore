#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Middleware;

internal sealed class MediaImageProcessingMiddleware : IMiddleware
{
    private static readonly HashSet<string> _imageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tga",
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

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var mediaOptions = _mediaOptions.Value;
        var requestPath = context.Request.Path;

        // Only handle requests under the media assets path.
        if (!requestPath.StartsWithSegments(mediaOptions.AssetsRequestPath, StringComparison.OrdinalIgnoreCase, out var remaining))
        {
            await next(context);
            return;
        }

        // Only handle image file requests.
        var ext = Path.GetExtension(remaining.Value);
        if (string.IsNullOrEmpty(ext) || !_imageExtensions.Contains(ext))
        {
            await next(context);
            return;
        }

        // Parse and validate resize commands from the query string.
        var commands = _commandParser.Parse(context);
        if (commands is null)
        {
            await next(context);
            return;
        }

        // If the format is not explicitly requested, default to the source file's format.
        if (string.IsNullOrEmpty(commands.Format))
            commands.Format = ExtensionToFormat(ext);

        var cacheKey = PhysicalFileSystemResizedImageCache.ComputeCacheKey(
            _shellSettings.Name,
            remaining.Value!,
            commands.GetValues());

        // Try the cache first.
        var cached = await _cache.GetAsync(cacheKey, context.RequestAborted);
        if (cached.HasValue)
        {
            await ServeAsync(context, cached.Value.Content, cached.Value.ContentType, mediaOptions, isNewResult: false);
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
            await ServeAsync(context, result.Output, result.ContentType, mediaOptions, isNewResult: true);
        }
    }

    private static async Task ServeAsync(HttpContext context, Stream content, string contentType, MediaOptions options, bool isNewResult)
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
            response.ContentLength = content.Length - content.Position;

        await content.CopyToAsync(response.Body, context.RequestAborted);
    }

    private static string ExtensionToFormat(string ext) => ext.TrimStart('.').ToLowerInvariant() switch
    {
        "jpeg" => "jpg",
        "png"  => "png",
        "gif"  => "gif",
        "webp" => "webp",
        "bmp"  => "bmp",
        _      => "jpg",
    };
}
