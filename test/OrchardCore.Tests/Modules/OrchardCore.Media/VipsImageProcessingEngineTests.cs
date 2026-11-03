#nullable enable

using NetVips;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Processing;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class VipsImageProcessingEngineTests
{
    private readonly VipsImageProcessingEngine _engine = new();

    // Creates a 3-band (RGB) black PNG of the given dimensions as a MemoryStream.
    private static MemoryStream CreateTestPng(int width, int height)
    {
        using var img = Image.Black(width, height, bands: 3);
        return new MemoryStream(img.WriteToBuffer(".png"));
    }

    // Reads width and height from PNG IHDR chunk (bytes 16-23, big-endian).
    private static (int Width, int Height) ReadPngDimensions(byte[] png)
    {
        var w = (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
        var h = (png[20] << 24) | (png[21] << 16) | (png[22] << 8) | png[23];
        return (w, h);
    }

    private static async Task<(int Width, int Height)> GetOutputPngDimensionsAsync(
        VipsImageProcessingEngine engine,
        MemoryStream input,
        ImageProcessingCommands commands,
        CancellationToken ct = default)
    {
        commands.Format = Format.Png;
        using var result = await engine.ProcessAsync(input, commands, ct);
        return ReadPngDimensions(((MemoryStream)result.Output).ToArray());
    }

    [Theory]
    [InlineData(200, 100, 100, null,  100, 50)]  // width-only constraint
    [InlineData(200, 100, null,  50,  100, 50)]  // height-only constraint
    [InlineData(200, 100, 80,  80,  80,  40)]    // both constraints, narrower axis wins
    public async Task Process_MaxMode_FitsWithinBoundsProportionally(
        int srcW, int srcH, int? reqW, int? reqH, int expW, int expH)
    {
        using var input = CreateTestPng(srcW, srcH);
        var commands = new ImageProcessingCommands { Width = reqW, Height = reqH, ResizeMode = ResizeMode.Max };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(expW, w);
        Assert.Equal(expH, h);
    }

    [Fact]
    public async Task Process_CropMode_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 60, Height = 60, ResizeMode = ResizeMode.Crop };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(60, w);
        Assert.Equal(60, h);
    }

    [Fact]
    public async Task Process_CropModeWithFocalPoint_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands
        {
            Width = 50,
            Height = 50,
            ResizeMode = ResizeMode.Crop,
            FocalPointX = 0.0f,
            FocalPointY = 0.0f,
        };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(50, w);
        Assert.Equal(50, h);
    }

    [Fact]
    public async Task Process_StretchMode_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 100, Height = 80, ResizeMode = ResizeMode.Stretch };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(80, h);
    }

    [Theory]
    [InlineData(200, 100, 50, null, 50, 100)]  // width-only: height is left unchanged
    [InlineData(200, 100, null, 40, 200, 40)]  // height-only: width is left unchanged
    public async Task Process_StretchModeSingleAxis_LeavesOtherAxisUnchanged(
        int srcW, int srcH, int? reqW, int? reqH, int expW, int expH)
    {
        using var input = CreateTestPng(srcW, srcH);
        var commands = new ImageProcessingCommands { Width = reqW, Height = reqH, ResizeMode = ResizeMode.Stretch };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(expW, w);
        Assert.Equal(expH, h);
    }

    [Fact]
    public async Task Process_CropModeWithFocalPointSingleAxis_LeavesOtherAxisUnchanged()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands
        {
            Width = 80,
            ResizeMode = ResizeMode.Crop,
            FocalPointX = 0.25f,
            FocalPointY = 0.75f,
        };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);

        // Width is constrained; the height axis is left at the source size.
        Assert.Equal(80, w);
        Assert.Equal(100, h);
    }

    [Fact]
    public async Task Process_PadMode_ProducessExactDimensions()
    {
        // Input 200x100 → pad to portrait 100x200; thumbnail shrinks to 100x50, rest is padding.
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands
        {
            Width = 100,
            Height = 200,
            ResizeMode = ResizeMode.Pad,
            BackgroundColor = "ffffff",
        };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(200, h);
    }

    [Fact]
    public async Task Process_MinMode_ScalesBySmallerAxisRatio()
    {
        // min(100/200, 100/100) = 0.5 → output 100x50
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 100, Height = 100, ResizeMode = ResizeMode.Min };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(50, h);
    }

    [Fact]
    public async Task Process_NoDimensions_RetainsOriginalSize()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Format = Format.Png };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        var (w, h) = ReadPngDimensions(((MemoryStream)result.Output).ToArray());
        Assert.Equal(200, w);
        Assert.Equal(100, h);
    }

    [Fact]
    public async Task Process_PngFormat_ReturnsCorrectContentType()
    {
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50, Format = Format.Png };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.Equal("image/png", result.ContentType);
    }

    [Fact]
    public async Task Process_WebpFormat_ReturnsCorrectContentType()
    {
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50, Format = Format.WebP };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.Equal("image/webp", result.ContentType);
    }

    [Fact]
    public async Task Process_GifFormat_ReturnsCorrectContentType()
    {
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50, Format = Format.Gif };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.Equal("image/gif", result.ContentType);
    }

    [Fact]
    public async Task Process_DefaultFormat_ReturnsJpeg()
    {
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50 };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.Equal("image/jpeg", result.ContentType);
    }

    [Fact]
    public async Task Process_InvalidQuality_UsesDefaultAndReturnsOutput()
    {
        // quality=0 is out of range (1-100) → engine uses default 85
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50, Quality = 0 };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.Equal("image/jpeg", result.ContentType);
        Assert.True(result.Output.Length > 0);
    }

    [Fact]
    public async Task Process_PadModeRgbaSource_ProducessExactDimensions()
    {
        // A 4-band (RGBA) source must pad without a band-count mismatch on the background color.
        using var rgba = Image.Black(200, 100, bands: 4);
        using var input = new MemoryStream(rgba.WriteToBuffer(".png"));

        var commands = new ImageProcessingCommands
        {
            Width = 100,
            Height = 200,
            ResizeMode = ResizeMode.Pad,
            BackgroundColor = "ffffff",
        };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(200, h);
    }

    [Fact]
    public async Task Process_PadModeGrayscaleSource_ProducessExactDimensions()
    {
        // A single-band (grayscale) source must pad without a band-count mismatch.
        using var gray = Image.Black(200, 100, bands: 1);
        using var input = new MemoryStream(gray.WriteToBuffer(".png"));

        var commands = new ImageProcessingCommands
        {
            Width = 100,
            Height = 200,
            ResizeMode = ResizeMode.Pad,
            BackgroundColor = "808080",
        };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(200, h);
    }

    [Fact]
    public async Task Process_BoxPadMode_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands
        {
            Width = 100,
            Height = 200,
            ResizeMode = ResizeMode.BoxPad,
            BackgroundColor = "ffffff",
        };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(200, h);
    }

    [Fact]
    public async Task Process_AutoOrientDisabled_ProducessOutput()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 100, ResizeMode = ResizeMode.Max, AutoOrient = false };
        var (w, h) = await GetOutputPngDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(50, h);
    }

    [Fact]
    public async Task Process_UndefinedFormat_FallsBackToJpeg()
    {
        // An unspecified format makes the engine fall back to JPEG.
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50, Format = Format.Undefined };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.Equal("image/jpeg", result.ContentType);
        Assert.True(result.Output.Length > 0);
    }

    [Fact]
    public async Task Process_OutputIsNonEmpty_Succeeds()
    {
        using var input = CreateTestPng(100, 50);
        var commands = new ImageProcessingCommands { Width = 50, Format = Format.Png };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        Assert.True(result.Output.Length > 0);
    }
}
