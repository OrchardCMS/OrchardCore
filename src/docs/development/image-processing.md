# Plan: Replacing ImageSharp with NetVips in OrchardCore

## Executive Summary

This is a **high-effort, high-risk** migration. The key distinction is that **SixLabors.ImageSharp.Web** is not merely an image-processing library — it is a complete ASP.NET Core on-demand image processing middleware with a provider/processor/cache pipeline. **NetVips** (C# bindings for libvips) is only an image-processing engine; it has no HTTP middleware at all. You would not be swapping one library for another — you would be **rebuilding an entire HTTP image-serving pipeline** and using NetVips as the pixel-manipulation engine inside it.

NetVips is the strongest engine choice for this workload. libvips uses a demand-driven, horizontally threaded pipeline that processes images in tiles rather than loading them entirely into memory, making it 4–8× faster than ImageSharp for typical web resize operations. It handles EXIF auto-orientation natively via a single `Autorot()` call, supports full GIF encode and decode, and is MIT licensed.

The primary motivation for this migration is the [SixLabors Split License](https://sixlabors.com/products/imagesharp/) introduced in ImageSharp v3, which requires a commercial license for sufficiently large commercial deployments.

---

## Part 1 — Full Scope of Impact

### 1.1 Directly Affected Projects

| Project | Package Removed | Notes |
|---|---|---|
| `OrchardCore.Media` | `SixLabors.ImageSharp.Web 3.2.0` | Core — the entire middleware pipeline lives here |
| `OrchardCore.Media.Azure` | `SixLabors.ImageSharp.Web.Providers.Azure 3.2.0` | Azure Blob Storage cache for resized images |
| `OrchardCore.Media.AmazonS3` | `SixLabors.ImageSharp.Web.Providers.AWS 3.2.0` | S3 cache for resized images |

### 1.2 Files That Must Be Replaced or Rewritten

| File | Lines | What It Does (ImageSharp-specific) |
|---|---|---|
| `src/OrchardCore.Modules/OrchardCore.Media/Startup.cs` | 46–49, 142–159, 220 | Wires up `AddImageSharp()`, `UseImageSharp()`, providers, processors, cache options |
| `src/OrchardCore.Modules/OrchardCore.Media/Processing/MediaImageSharpConfiguration.cs` | Entire file | Configures `ImageSharpMiddlewareOptions` — command validation, token enforcement, auto-orient default, secure file cache headers |
| `src/OrchardCore.Modules/OrchardCore.Media/Processing/MediaResizingFileProvider.cs` | Entire file | Implements `IImageProvider` — bridges OrchardCore's media store to ImageSharp's pipeline |
| `src/OrchardCore.Modules/OrchardCore.Media/Processing/TokenCommandProcessor.cs` | Entire file | Implements `IImageWebProcessor` — strips/validates token from command bag |
| `src/OrchardCore.Modules/OrchardCore.Media/Processing/ImageVersionProcessor.cs` | Entire file | Implements `IImageWebProcessor` — strips version parameter from command bag |
| `src/OrchardCore.Modules/OrchardCore.Media/Services/ResizedMediaCacheBackgroundTask.cs` | Lines 6–7, all cache cleanup logic | Uses `IImageSharpMiddlewareOptions` and `.meta` file tracking to delete stale cache |
| `src/OrchardCore.Modules/OrchardCore.Media.Azure/Startup.cs` | 20–21 | Registers `AzureBlobStorageCache` |
| `src/OrchardCore.Modules/OrchardCore.Media.Azure/Services/AzureBlobStorageCacheOptionsConfiguration.cs` | Entire file | Configures `AzureBlobStorageCacheOptions` (ImageSharp type) |
| `src/OrchardCore.Modules/OrchardCore.Media.Azure/Services/ImageSharpBlobImageCacheTenantEvents.cs` | Entire file | Clears Azure blob cache on tenant events |
| `src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/Startup.cs` | 21–22 | Registers `AWSS3StorageCache` |
| `src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/Services/AWSS3StorageCacheOptionsConfiguration.cs` | Entire file | Configures `AWSS3StorageCacheOptions` (ImageSharp type) |
| `src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/Services/ImageSharpS3ImageCacheBucketTenantEvents.cs` | Entire file | Clears S3 cache on tenant events |
| `test/OrchardCore.Tests/Modules/OrchardCore.Media/MediaTokenTests.cs` | Line 5 | Uses `SixLabors.ImageSharp.Web.Processors` for command names |

### 1.3 Files That Are Safe — No ImageSharp Dependency

These are **untouched** by the migration because they operate at the URL-parameter / abstraction layer:

- `Processing/ImageSharpUrlFormatter.cs` — builds query strings, no ImageSharp types (despite the name)
- `Filters/ResizeUrlFilter.cs` — Liquid filter, calls URL formatter
- `TagHelpers/ImageResizeTagHelper.cs` — Razor tag helper, uses `MediaCommands` and `IMediaTokenService`
- `OrchardCore.Media.Core/Processing/MediaCommands.cs` — plain DTO
- `OrchardCore.Media.Core/Processing/ResizeMode.cs` — custom enum
- `OrchardCore.Media.Core/Processing/Format.cs` — custom enum
- `Processing/MediaTokenService.cs` — abstraction
- All shortcodes, Liquid tags, Razor helper extensions, media profile services

---

## Part 2 — What Must Be Built from Scratch

### 2.1 The Core Problem: No NetVips.Web Exists

ImageSharp.Web provides a complete, battle-tested middleware pipeline:

```
HTTP Request
    → IImageProvider.IsMatch() / IsValidRequest()
    → IImageProvider.GetAsync() → IImageResolver (stream)
    → OnParseCommandsAsync (command validation, token, security)
    → IImageWebProcessor[] (each processor handles its commands)
    → IImageCache (check hit/miss)
    → SixLabors.ImageSharp (actual pixel operations)
    → IImageCache.SetAsync (write to cache)
    → HTTP Response (with Cache-Control headers)
```

NetVips has no equivalent. You must build every layer yourself, using NetVips only for the pixel-operation step.

### 2.2 New Components Required

| New Component | Type | Replaces |
|---|---|---|
| `MediaImageProcessingMiddleware` | `IMiddleware` | `app.UseImageSharp()` |
| `IImageCommandParser` | Interface | `ImageSharpMiddlewareOptions.OnParseCommandsAsync` |
| `MediaCommandParser` | Implementation | Command string parsing, token validation, size validation |
| `IImageProcessingEngine` | Interface | The ImageSharp pixel-operation engine |
| `VipsImageProcessingEngine` | Implementation | Resize, crop, format convert, EXIF auto-orient, background color via libvips |
| `IResizedImageCache` | Interface | `IImageCache` (read/write cache) |
| `PhysicalFileSystemResizedImageCache` | Implementation | `PhysicalFileSystemCache` from ImageSharp |
| `AzureBlobResizedImageCache` | Implementation (Media.Azure) | `AzureBlobStorageCache` |
| `AWSS3ResizedImageCache` | Implementation (Media.AmazonS3) | `AWSS3StorageCache` |
| `IImageFormatDetector` | Interface | `FormatUtilities` from ImageSharp |
| `MediaImageFormatDetector` | Implementation | Uses file extension + magic bytes to detect format |

### 2.3 NetVips Operation Mapping

Every operation currently delegated to ImageSharp's processors maps cleanly to libvips:

| Operation | ImageSharp API | NetVips Equivalent | Notes |
|---|---|---|---|
| Resize (max) | `ResizeWebProcessor` + `ResizeOptions{Mode=Max}` | `image.ThumbnailImage(w, height: h, size: Enums.Size.Both, crop: Enums.Interesting.None)` | Fits within box, maintains aspect |
| Resize (crop) | `ResizeMode.Crop` | `image.ThumbnailImage(w, height: h, crop: Enums.Interesting.Centre)` | Use `Attention` for content-aware crop |
| Resize (pad) | `ResizeMode.Pad` | `ThumbnailImage` + `image.Embed(x, y, w, h, extend: Enums.Extend.Background, background: color)` | |
| Resize (boxpad) | `ResizeMode.BoxPad` | Same as Pad, centered | |
| Resize (min) | `ResizeMode.Min` | `ThumbnailImage(w, height: h, size: Enums.Size.Down)` | Only shrink, never enlarge |
| Resize (stretch) | `ResizeMode.Stretch` | `image.Resize(hscale, vscale: vscale)` | Ignores aspect ratio |
| Auto-orient (EXIF) | `AutoOrientWebProcessor` | `image.Autorot()` | **Built-in, single call, no extra library** |
| Format to JPEG | `JpegEncoder{Quality=n}` | `image.WriteToBuffer(".jpg[Q=n]")` | libjpeg-turbo under the hood |
| Format to PNG | `PngEncoder` | `image.WriteToBuffer(".png")` | |
| Format to WebP | `WebPEncoder{Quality=n}` | `image.WriteToBuffer(".webp[Q=n]")` | |
| Format to GIF | `GifEncoder` | `image.WriteToBuffer(".gif")` | **Built-in, no gap** |
| Format to BMP | (BmpEncoder) | `image.WriteToBuffer(".bmp")` | |
| Background color | `BackgroundColorWebProcessor` | `background` parameter on `Embed()` | `double[] { r, g, b }` |
| Focal point anchor | x/y crop coordinate | `crop: Enums.Interesting.Centre` + custom `x`/`y` offset on `Embed` | |

**No critical gaps.** Unlike SkiaSharp, NetVips handles EXIF orientation natively and supports full GIF encode/decode.

#### Why `Thumbnail` instead of `Resize`

libvips' `Thumbnail` operation uses **shrink-on-load**: for JPEG it tells libjpeg-turbo to shrink while decoding (using the DCT coefficients directly), for TIFF it reads reduced resolution subfiles, for WebP it uses the codec's own downscaling. This means a 20 MP JPEG being resized to 400px wide is decoded at roughly ¼ resolution — the full pixel data is never in memory. This is the primary reason libvips is 4–8× faster than ImageSharp for web resize workloads.

---

## Part 3 — Phased Implementation Plan

### Phase 0 — Preparation and Abstraction (Week 1–2)

**Goal**: Create the abstraction boundary that allows the processing engine to be swapped without touching the public API surface.

**Step 0.1** — Introduce `IImageProcessingEngine` in `OrchardCore.Media.Abstractions`:

```csharp
// New file: src/OrchardCore/OrchardCore.Media.Abstractions/IImageProcessingEngine.cs
public interface IImageProcessingEngine
{
    Task<ImageProcessingResult> ProcessAsync(
        Stream input,
        MediaCommands commands,
        CancellationToken cancellationToken = default);
}

public sealed class ImageProcessingResult : IDisposable
{
    public Stream Output { get; init; }
    public string ContentType { get; init; }
    public void Dispose() => Output?.Dispose();
}
```

**Step 0.2** — Introduce `IResizedImageCache` in `OrchardCore.Media.Abstractions`:

```csharp
public interface IResizedImageCache
{
    Task<Stream> GetAsync(string cacheKey);
    Task SetAsync(string cacheKey, Stream image, string contentType, TimeSpan maxAge);
    Task<bool> ExistsAsync(string cacheKey);
    Task ClearAsync(string prefix = null);
    Task<IEnumerable<CachedImageEntry>> GetStaleEntriesAsync(TimeSpan maxAge);
}
```

**Step 0.3** — Introduce `IImageFormatDetector`:

```csharp
public interface IImageFormatDetector
{
    bool IsImageRequest(HttpContext context);
    string GetContentType(string extension);
}
```

**Deliverable**: Clean abstraction layer with no ImageSharp or NetVips references. Zero behavior change — ImageSharp still runs everything at this stage.

---

### Phase 1 — Build the NetVips Processing Engine (Week 3–4)

**Goal**: Implement `IImageProcessingEngine` using NetVips in a new internal class. Do not wire it into the middleware yet — test it in isolation.

**Step 1.1** — Add NuGet packages to `OrchardCore.Media`:

```xml
<PackageReference Include="NetVips" Version="2.*" />
<PackageReference Include="NetVips.Native" Version="8.*" />
```

`NetVips.Native` is a single meta-package that bundles the pre-built libvips native binary for the current platform (Windows x64, Linux x64/arm64, macOS x64/arm64). No separate per-platform packages are needed, though platform-specific variants (`NetVips.Native.win-x64`, etc.) can be used to reduce deployment size if targeting a single platform.

> **Note**: `NetVips.Native` includes libvips and all its dependencies (libjpeg-turbo, libpng, libwebp, libgif, libtiff, libheif, etc.) — roughly 15–30 MB depending on platform. This is larger than SkiaSharp's native footprint but brings complete codec coverage with no further dependencies.

**Step 1.2** — Implement `VipsImageProcessingEngine`:

The engine handles these steps in order:

1. Load the input stream via `Image.NewFromStream(stream)`
2. Call `image.Autorot()` — libvips reads and applies EXIF orientation, then strips the tag
3. Resolve target dimensions (honour only-width, only-height, or both)
4. Apply the `ResizeMode` via `ThumbnailImage` or `Resize` as appropriate
5. Apply background color padding if `ResizeMode` is `Pad` or `BoxPad`
6. Encode to the requested format via `WriteToBuffer(formatString)`
7. Return the encoded bytes as a `MemoryStream` with the correct `ContentType`

```csharp
public sealed class VipsImageProcessingEngine : IImageProcessingEngine
{
    public Task<ImageProcessingResult> ProcessAsync(
        Stream input, MediaCommands commands, CancellationToken cancellationToken = default)
    {
        // NetVips is synchronous internally — offload to thread pool to avoid blocking async context
        return Task.Run(() =>
        {
            using var image = Image.NewFromStream(input);
            using var oriented = image.Autorot();

            var processed = ApplyResize(oriented, commands);

            var formatString = BuildFormatString(commands);
            var bytes = processed.WriteToBuffer(formatString);

            return new ImageProcessingResult
            {
                Output = new MemoryStream(bytes),
                ContentType = GetContentType(commands.Format),
            };
        }, cancellationToken);
    }
}
```

**Step 1.3** — Implement `ResizeMode` geometry:

```csharp
private static Image ApplyResize(Image image, MediaCommands commands)
{
    var w = commands.Width ?? 0;
    var h = commands.Height ?? 0;

    return commands.ResizeMode switch
    {
        ResizeMode.Max     => image.ThumbnailImage(w == 0 ? int.MaxValue : w,
                                  height: h == 0 ? int.MaxValue : h,
                                  size: Enums.Size.Down,
                                  crop: Enums.Interesting.None),

        ResizeMode.Min     => image.ThumbnailImage(w == 0 ? int.MaxValue : w,
                                  height: h == 0 ? int.MaxValue : h,
                                  size: Enums.Size.Down,   // never upscale
                                  crop: Enums.Interesting.None),

        ResizeMode.Crop    => image.ThumbnailImage(w, height: h,
                                  size: Enums.Size.Both,
                                  crop: Enums.Interesting.Centre),

        ResizeMode.Stretch => image.Resize(
                                  w > 0 ? (double)w / image.Width : 1.0,
                                  vscale: h > 0 ? (double)h / image.Height : 1.0),

        ResizeMode.Pad or ResizeMode.BoxPad
                           => ApplyPad(image, w, h, commands.BackgroundColor),

        _                  => image.ThumbnailImage(w == 0 ? int.MaxValue : w,
                                  height: h == 0 ? int.MaxValue : h,
                                  size: Enums.Size.Down,
                                  crop: Enums.Interesting.None),
    };
}

private static Image ApplyPad(Image image, int targetW, int targetH, string bgColor)
{
    var thumb = image.ThumbnailImage(targetW, height: targetH,
        size: Enums.Size.Both, crop: Enums.Interesting.None);

    var bg = ParseColor(bgColor);  // double[] { r, g, b }
    var x = (targetW - thumb.Width)  / 2;
    var y = (targetH - thumb.Height) / 2;

    return thumb.Embed(x, y, targetW, targetH,
        extend: Enums.Extend.Background, background: bg);
}
```

**Step 1.4** — Implement format encoding strings:

```csharp
private static string BuildFormatString(MediaCommands commands)
{
    var quality = commands.Quality > 0 ? commands.Quality : 85;
    return commands.Format switch
    {
        Format.Jpg  => $".jpg[Q={quality}]",
        Format.Png  => ".png",
        Format.WebP => $".webp[Q={quality}]",
        Format.Gif  => ".gif",
        Format.Bmp  => ".bmp",
        _           => $".jpg[Q={quality}]",
    };
}
```

**Step 1.5** — Write unit tests for the engine:

- One test per `ResizeMode` asserting correct output pixel dimensions
- EXIF auto-orient: feed a rotated JPEG, assert output has correct width/height and no orientation tag
- GIF encode: assert output is a valid GIF
- Format conversion: JPEG → WebP, assert `Content-Type`
- Quality parameter: two JPEG encodes at Q=10 and Q=90, assert Q=90 produces larger file

---

### Phase 2 — Build the Custom HTTP Middleware (Week 5–7)

**Goal**: Replace `app.UseImageSharp()` with a custom middleware that reproduces the ImageSharp.Web pipeline using the new abstractions.

**Step 2.1** — Implement `MediaImageProcessingMiddleware`:

```csharp
public class MediaImageProcessingMiddleware : IMiddleware
{
    // Dependencies:
    //   IImageFormatDetector     — is this an image request at the media path?
    //   IMediaFileProvider       — read source image
    //   IImageCommandParser      — validate/sanitize query string → MediaCommands
    //   IImageProcessingEngine   — process image bytes
    //   IResizedImageCache       — cache lookup and store
    //   IOptions<MediaOptions>   — cache durations, CDN, etc.

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_formatDetector.IsImageRequest(context))
        {
            await next(context);
            return;
        }

        var commands = _commandParser.Parse(context);
        if (commands is null || commands.IsEmpty)
        {
            await next(context);
            return;
        }

        var cacheKey = ComputeCacheKey(context.Request.Path, commands);
        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            await ServeStreamAsync(context, cached, commands);
            return;
        }

        var path = GetMediaPath(context);
        var fileInfo = _mediaFileProvider.GetFileInfo(path);
        if (!fileInfo.Exists)
        {
            await next(context);
            return;
        }

        using var source = fileInfo.CreateReadStream();
        using var result = await _engine.ProcessAsync(source, commands);

        var cacheStream = new MemoryStream();
        await result.Output.CopyToAsync(cacheStream);
        cacheStream.Position = 0;

        await _cache.SetAsync(cacheKey, cacheStream, result.ContentType,
            TimeSpan.FromDays(_options.MaxCacheDays));

        cacheStream.Position = 0;
        await ServeStreamAsync(context, cacheStream, commands);
    }
}
```

**Step 2.2** — Implement `MediaCommandParser` (replacing `MediaImageSharpConfiguration.OnParseCommandsAsync`):

This class reads `HttpContext.Request.Query`, maps to `MediaCommands`, and applies the same validation logic currently in `MediaImageSharpConfiguration`:

- Token validation (HMAC check via `IMediaTokenService` — unchanged)
- Strip unsupported raw commands
- Enforce `SupportedSizes` for tokenless requests
- Strip `xy` and `bgcolor` for tokenless requests
- Default `mode=max` if no mode supplied
- Default `autoorient=true` if any commands are present (NetVips always auto-orients regardless, but this preserves the command semantics for existing cached URLs)

The command key names are currently taken from ImageSharp constants like `ResizeWebProcessor.Width`. After migration, define these as string constants in a new `MediaCommandKeys` static class in `OrchardCore.Media.Core`:

```csharp
public static class MediaCommandKeys
{
    public const string Width      = "width";
    public const string Height     = "height";
    public const string Mode       = "rmode";
    public const string Quality    = "quality";
    public const string Format     = "format";
    public const string Xy         = "xy";
    public const string Color      = "bgcolor";
    public const string AutoOrient = "autoorient";
    public const string Version    = "v";
    public const string Token      = "token";
}
```

> **Critical**: These key names must match exactly what `Processing/ImageSharpUrlFormatter.cs` emits — that file is not changing. Verify each constant against the formatter before committing.

**Step 2.3** — Implement cache key computation:

ImageSharp currently uses `BackwardsCompatibleCacheKey` to prefix cache entries with the tenant name, preventing cross-tenant leakage. Replicate this: the key must include `ShellSettings.Name` as a prefix, then SHA-256 of `path + sorted query string parameters`.

**Step 2.4** — Implement `PhysicalFileSystemResizedImageCache`:

Store processed images in a folder under the shell's data path. Use `media-cache` as the folder name (distinct from ImageSharp's `is-cache`). Structure:

```
{shellName}/media-cache/{cacheKey[0..1]}/{cacheKey}.{ext}
```

No `.meta` sidecar files — metadata (content type, creation time) is stored in the file's extension and filesystem timestamps, removing the `.meta`-scanning dependency that `ResizedMediaCacheBackgroundTask` currently has.

**Step 2.5** — Wire up in `Startup.cs`:

```csharp
// Remove:
services.AddImageSharp()...;
services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, MediaImageSharpConfiguration>();

// Add:
services.AddSingleton<IImageProcessingEngine, VipsImageProcessingEngine>();
services.AddSingleton<IImageCommandParser, MediaCommandParser>();
services.AddSingleton<IResizedImageCache, PhysicalFileSystemResizedImageCache>();
services.AddSingleton<IImageFormatDetector, MediaImageFormatDetector>();
services.AddTransient<MediaImageProcessingMiddleware>();

// In Configure():
// Remove:  app.UseImageSharp();
// Add:     app.UseMiddleware<MediaImageProcessingMiddleware>();
```

Note: `VipsImageProcessingEngine` is `Singleton` safe because NetVips has no per-request state; libvips manages its own thread pool internally.

---

### Phase 3 — Replace Cloud Cache Providers (Week 8–9)

**Goal**: Migrate `OrchardCore.Media.Azure` and `OrchardCore.Media.AmazonS3` to use `IResizedImageCache` instead of ImageSharp's cache abstractions.

**Step 3.1** — Implement `AzureBlobResizedImageCache : IResizedImageCache` in `OrchardCore.Media.Azure`:

This replaces `AzureBlobStorageCache`. Use the existing Azure SDK already present in that project to read/write blobs. The blob name mirrors the local cache key structure.

Remove:
- `AzureBlobStorageCacheOptionsConfiguration.cs`
- `ImageSharpBlobImageCacheTenantEvents.cs` (rewrite using `IResizedImageCache.ClearAsync()`)

Add:
- `AzureBlobResizedImageCache.cs`
- `AzureBlobResizedImageCacheOptions.cs` (replaces `ImageSharpBlobImageCacheOptions`)

**Step 3.2** — Implement `AWSS3ResizedImageCache : IResizedImageCache` in `OrchardCore.Media.AmazonS3`:

Same pattern. Use the existing AWSSDK already in that project.

Remove:
- `AWSS3StorageCacheOptionsConfiguration.cs`
- `ImageSharpS3ImageCacheBucketTenantEvents.cs` (rewrite)

Add:
- `AWSS3ResizedImageCache.cs`
- `AWSS3ResizedImageCacheOptions.cs`

**Step 3.3** — Update `ResizedMediaCacheBackgroundTask.cs`:

Currently uses `IImageSharpMiddlewareOptions` and scans for `.meta` files. After migration, replace with calls to `IResizedImageCache.GetStaleEntriesAsync()`. The background task schedule (`0 0 * * *`), logging, and `ResizedCacheMaxStale` option are unchanged.

---

### Phase 4 — Remove ImageSharp Packages (Week 10)

**Step 4.1** — Remove from `OrchardCore.Media.csproj`:

```xml
<PackageReference Include="SixLabors.ImageSharp.Web" Version="3.2.0" />
```

**Step 4.2** — Remove from `OrchardCore.Media.Azure.csproj`:

```xml
<PackageReference Include="SixLabors.ImageSharp.Web.Providers.Azure" Version="3.2.0" />
```

**Step 4.3** — Remove from `OrchardCore.Media.AmazonS3.csproj`:

```xml
<PackageReference Include="SixLabors.ImageSharp.Web.Providers.AWS" Version="3.2.0" />
```

**Step 4.4** — Fix `test/OrchardCore.Tests/Modules/OrchardCore.Media/MediaTokenTests.cs` line 5: remove `using SixLabors.ImageSharp.Web.Processors` and replace command key string references with `MediaCommandKeys` constants.

---

### Phase 5 — Testing and Validation (Week 11–12)

**Step 5.1** — Existing tests that must still pass:

- `MediaTokenTests` — token generation and HMAC validation (engine-agnostic)
- `MediaCommandsTests` — command DTO parsing
- `MediaOrchardHelperExtensionsTests` — URL generation
- `ImageShortcodeTests`, `AssetUrlShortcodeTests` — shortcode output

**Step 5.2** — New unit tests:

- `VipsImageProcessingEngineTests` — one test per `ResizeMode` asserting output dimensions, EXIF strip, GIF encode, format conversion
- `MediaCommandParserTests` — token validation, size validation, command stripping, tokenless mode restrictions
- `PhysicalFileSystemResizedImageCacheTests` — cache hit, miss, stale entry detection

**Step 5.3** — Integration tests:

- HTTP GET for a media image with resize parameters: assert correct status code, `Content-Type`, and pixel dimensions in response body
- Cache hit path: second identical request must be served from cache (assert no engine call)
- Token security: request with invalid token must produce the unprocessed image
- Multitenancy: resized cache from tenant A must not be served to tenant B

**Step 5.4** — Visual regression testing (human review required):

Compare output images between ImageSharp and NetVips for a reference set with known inputs. Pay attention to:

- JPEG colour accuracy (libvips uses libjpeg-turbo's colour space handling, which differs slightly from ImageSharp's managed decoder)
- Crop focal point positioning
- Pad background colour fill
- EXIF orientation correction for all 8 EXIF orientations

---

## Part 4 — Risks and Mitigations

| Risk | Severity | Mitigation |
|---|---|---|
| **NetVips.Native binary size** | Low | ~15–30 MB per platform. Acceptable for a server deployment. Use platform-specific packages to reduce if needed. |
| **NetVips native library missing at runtime** | High | `NetVips.Native` NuGet handles deployment automatically. Add a startup health check that calls `Image.NewFromArray(new[] { 0 })` to verify libvips loads before accepting traffic. |
| **Cache invalidation on upgrade** | Medium | The old `is-cache` folder (ImageSharp `.meta` format) is incompatible. On first deployment the new `media-cache` is cold — plan during low traffic. Old `is-cache` can be deleted after deployment. |
| **Colour rendering differences** | Low | libvips and ImageSharp have different colour management defaults. Run visual regression tests before go-live. |
| **Resize quality differences** | Low | libvips uses lanczos3 by default for `Thumbnail`, which is comparable to ImageSharp's Lanczos resampler. Visually equivalent in practice. |
| **Command key name mismatch** | High | If a `MediaCommandKeys` constant doesn't match what `ImageSharpUrlFormatter` emits, existing cached/bookmarked URLs will silently produce unprocessed images. Cross-reference every constant against the formatter before deploying. |
| **Thread safety of VipsImage objects** | Medium | NetVips `Image` objects are not thread-safe between instances but the library's internal thread pool is safe. Register `VipsImageProcessingEngine` as `Singleton`, keep `Image` instances scoped to each `ProcessAsync` call. |
| **libvips version mismatch** | Low | `NetVips.Native` version must match the `NetVips` binding version. Keep both pinned to the same minor version in the `.csproj`. |

---

## Part 5 — Dependency Footprint Comparison

| | ImageSharp.Web 3.2 | NetVips 2.x + Native |
|---|---|---|
| **License** | SixLabors Split (commercial) | MIT |
| **Native code** | None (pure managed) | Yes — libvips + bundled codecs |
| **Memory model** | Managed heap | Demand-driven tiles; far lower peak RAM for large images |
| **GIF support** | Full encode + decode | Full encode + decode |
| **EXIF handling** | Built-in | Built-in (`Autorot()`) |
| **HTTP middleware** | Built-in (`ImageSharp.Web`) | None — must be built |
| **Azure/AWS cache** | Official packages | Must be built |
| **Benchmark (typical web resize)** | ~120ms | ~15–30ms (4–8× faster) |
| **Animated image support** | Animated GIF/WebP | Animated GIF/WebP/APNG |

---

## Part 6 — Alternatives Considered

**Alternative A — Stay on ImageSharp 2.x** (Apache 2.0 licensed)

If the license is the only concern, rolling back to v2 avoids the issue entirely and requires only a NuGet version change. The `ImageSharp.Web` API between v2 and v3 is largely compatible. This is the cheapest mitigation by far.

**Alternative B — SkiaSharp**

SkiaSharp (C# bindings to Google's Skia) is faster than ImageSharp but slower than NetVips for web resize workloads. It does not handle EXIF orientation natively (requires `MetadataExtractor`) and cannot encode GIF. It requires the same custom middleware rebuild as NetVips. NetVips is the better engine choice for this specific use case.

**Alternative C — Magick.NET**

ImageMagick bindings. Most feature-complete option (every format, every operation), but the slowest of the three alternatives. Suitable if rich image manipulation beyond resize/crop/convert is needed.

---

## Effort Estimate

| Phase | Effort | Parallelizable? |
|---|---|---|
| Phase 0 — Abstractions | 2–3 days | No — must come first |
| Phase 1 — NetVips engine | 3–5 days | Partially — tests can run in parallel with engine code |
| Phase 2 — HTTP middleware | 6–10 days | No — depends on Phase 1 |
| Phase 3 — Cloud cache providers | 3–5 days | Yes — Azure and AWS can be done in parallel |
| Phase 4 — Remove packages | 1 day | No |
| Phase 5 — Testing | 4–6 days | Partially |
| **Total** | **19–30 working days** | |

NetVips reduces Phase 1 effort (vs SkiaSharp) because EXIF and GIF require no workarounds. The total estimate is 4–6 weeks for one developer, or 3–4 weeks with two developers working Phases 1 and 3 in parallel.
