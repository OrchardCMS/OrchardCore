#nullable enable

using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.ImageSharpV3.Engine;
using OrchardCore.Media.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class ImageSharpImageProcessingEngineTests
{
    private readonly ImageSharpImageProcessingEngine _engine = new();

    // Creates an RGBA PNG of the given dimensions as a MemoryStream.
    private static MemoryStream CreateTestPng(int width, int height)
    {
        using var img = new Image<Rgba32>(width, height);
        var stream = new MemoryStream();
        img.SaveAsPng(stream);
        stream.Position = 0;
        return stream;
    }

    private static async Task<(int Width, int Height)> GetOutputDimensionsAsync(
        ImageSharpImageProcessingEngine engine,
        MemoryStream input,
        ImageProcessingCommands commands,
        CancellationToken ct = default)
    {
        commands.Format = Format.Png;
        using var result = await engine.ProcessAsync(input, commands, ct);
        using var output = Image.Load(result.Output);
        return (output.Width, output.Height);
    }

    [Theory]
    [InlineData(200, 100, 100, null, 100, 50)]  // width-only constraint
    [InlineData(200, 100, null, 50, 100, 50)]   // height-only constraint
    [InlineData(200, 100, 80, 80, 80, 40)]      // both constraints, narrower axis wins
    public async Task Process_MaxMode_FitsWithinBoundsProportionally(
        int srcW, int srcH, int? reqW, int? reqH, int expW, int expH)
    {
        using var input = CreateTestPng(srcW, srcH);
        var commands = new ImageProcessingCommands { Width = reqW, Height = reqH, ResizeMode = ResizeMode.Max };
        var (w, h) = await GetOutputDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(expW, w);
        Assert.Equal(expH, h);
    }

    [Fact]
    public async Task Process_CropMode_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 60, Height = 60, ResizeMode = ResizeMode.Crop };
        var (w, h) = await GetOutputDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
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
        var (w, h) = await GetOutputDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(50, w);
        Assert.Equal(50, h);
    }

    [Fact]
    public async Task Process_StretchMode_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 100, Height = 80, ResizeMode = ResizeMode.Stretch };
        var (w, h) = await GetOutputDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(80, h);
    }

    [Fact]
    public async Task Process_PadMode_ProducessExactDimensions()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands
        {
            Width = 100,
            Height = 200,
            ResizeMode = ResizeMode.Pad,
            BackgroundColor = "ffffff",
        };
        var (w, h) = await GetOutputDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(200, h);
    }

    [Fact]
    public async Task Process_NoDimensions_RetainsOriginalSize()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Format = Format.Png };
        using var result = await _engine.ProcessAsync(input, commands, TestContext.Current.CancellationToken);
        using var output = Image.Load(result.Output);
        Assert.Equal(200, output.Width);
        Assert.Equal(100, output.Height);
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
    public async Task Process_AutoOrientDisabled_ProducessOutput()
    {
        using var input = CreateTestPng(200, 100);
        var commands = new ImageProcessingCommands { Width = 100, ResizeMode = ResizeMode.Max, AutoOrient = false };
        var (w, h) = await GetOutputDimensionsAsync(_engine, input, commands, TestContext.Current.CancellationToken);
        Assert.Equal(100, w);
        Assert.Equal(50, h);
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
