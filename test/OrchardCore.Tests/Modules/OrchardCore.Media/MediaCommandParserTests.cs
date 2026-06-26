#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Media;
using OrchardCore.Media.Models;
using OrchardCore.Media.Processing;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class MediaCommandParserTests
{
    private static readonly int[] _supportedSizes = [16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048];

    private static MediaCommandParser CreateParser(
        bool validateToken = false,
        bool useTokenizedQueryString = true,
        int[]? supportedSizes = null)
    {
        var tokenServiceMock = new Mock<IMediaTokenService>();
        tokenServiceMock
            .Setup(ts => ts.TryValidateToken(
                It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                It.IsAny<string>()))
            .Returns(validateToken);

        var mediaOptions = Options.Create(new MediaOptions
        {
            UseTokenizedQueryString = useTokenizedQueryString,
            SupportedSizes = supportedSizes ?? _supportedSizes,
        });

        return new MediaCommandParser(tokenServiceMock.Object, mediaOptions);
    }

    private static DefaultHttpContext CreateContext(params (string key, string value)[] queryParams)
    {
        var context = new DefaultHttpContext();
        context.Request.Query = new QueryCollection(
            queryParams.ToDictionary(p => p.key, p => new StringValues(p.value)));
        return context;
    }

    [Fact]
    public void Parse_EmptyQuery_ReturnsNull()
    {
        var parser = CreateParser();
        Assert.Null(parser.Parse(new DefaultHttpContext()));
    }

    [Fact]
    public void Parse_VersionOnly_ReturnsNull()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("v", "20240101"));
        Assert.Null(parser.Parse(ctx));
    }

    [Fact]
    public void Parse_UnknownParamsOnly_ReturnsNull()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("foo", "bar"), ("baz", "qux"));
        Assert.Null(parser.Parse(ctx));
    }

    [Fact]
    public void Parse_SupportedWidthNoToken_ReturnsCommands()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("width", "1024"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("1024", commands!.Width);
        Assert.Equal("max", commands.ResizeMode);
    }

    [Fact]
    public void Parse_UnsupportedWidthNoToken_ReturnsNull()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("width", "999"));
        Assert.Null(parser.Parse(ctx));
    }

    [Fact]
    public void Parse_UnsupportedWidthValidToken_ReturnsCommands()
    {
        var parser = CreateParser(validateToken: true);
        var ctx = CreateContext(("width", "999"), ("token", "sometoken"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("999", commands!.Width);
    }

    [Fact]
    public void Parse_InvalidToken_ReturnsNull()
    {
        // validateToken=false means TryValidateToken returns false for any token.
        var parser = CreateParser(validateToken: false);
        var ctx = CreateContext(("width", "1024"), ("token", "badtoken"));
        Assert.Null(parser.Parse(ctx));
    }

    [Fact]
    public void Parse_FocalPointNoToken_IsStripped()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("width", "1024"), ("rxy", "0.5,0.5"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Null(commands!.ResizeFocalPoint);
        Assert.Equal("1024", commands.Width);
    }

    [Fact]
    public void Parse_FocalPointValidToken_IsPreserved()
    {
        var parser = CreateParser(validateToken: true);
        var ctx = CreateContext(("width", "1024"), ("rxy", "0.7,0.3"), ("token", "tok"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("0.7,0.3", commands!.ResizeFocalPoint);
    }

    [Fact]
    public void Parse_BackgroundColorNoToken_IsStripped()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("width", "1024"), ("bgcolor", "ff0000"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Null(commands!.BackgroundColor);
    }

    [Fact]
    public void Parse_BackgroundColorValidToken_IsPreserved()
    {
        var parser = CreateParser(validateToken: true);
        var ctx = CreateContext(("width", "600"), ("bgcolor", "0000ff"), ("token", "tok"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("0000ff", commands!.BackgroundColor);
    }

    [Fact]
    public void Parse_DefaultResizeModeIsMax_Succeeds()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("width", "600"));
        var commands = parser.Parse(ctx);
        Assert.Equal("max", commands!.ResizeMode);
    }

    [Fact]
    public void Parse_ExplicitResizeMode_Preservesd()
    {
        var parser = CreateParser(validateToken: true);
        var ctx = CreateContext(("width", "600"), ("rmode", "crop"), ("token", "tok"));
        var commands = parser.Parse(ctx);
        Assert.Equal("crop", commands!.ResizeMode);
    }

    [Fact]
    public void Parse_MatOnly_ReturnsCommands()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("format", "png"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("png", commands!.Format);
    }

    [Fact]
    public void Parse_QualityOnly_ReturnsCommands()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("quality", "75"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("75", commands!.Quality);
    }

    [Fact]
    public void Parse_SupportedHeightWithUnsupportedWidth_KeepssHeight()
    {
        var parser = CreateParser();
        // width=999 stripped, height=600 kept → commands not null
        var ctx = CreateContext(("width", "999"), ("height", "600"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Null(commands!.Width);
        Assert.Equal("600", commands.Height);
    }

    [Fact]
    public void Parse_BothDimensionsUnsupportedNoToken_ReturnsNull()
    {
        var parser = CreateParser();
        var ctx = CreateContext(("width", "999"), ("height", "300"));
        Assert.Null(parser.Parse(ctx));
    }

    [Fact]
    public void Parse_TokenizationDisabledSupportedSize_ReturnsCommands()
    {
        var parser = CreateParser(useTokenizedQueryString: false);
        var ctx = CreateContext(("width", "1024"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Equal("1024", commands!.Width);
    }

    [Fact]
    public void Parse_TokenizationDisabledUnsupportedSize_ReturnsNull()
    {
        var parser = CreateParser(useTokenizedQueryString: false);
        var ctx = CreateContext(("width", "999"));
        Assert.Null(parser.Parse(ctx));
    }

    [Fact]
    public void Parse_TokenizationDisabledFocalPoint_IsStripped()
    {
        var parser = CreateParser(useTokenizedQueryString: false);
        var ctx = CreateContext(("width", "1024"), ("rxy", "0.5,0.5"));
        var commands = parser.Parse(ctx);
        Assert.NotNull(commands);
        Assert.Null(commands!.ResizeFocalPoint);
    }
}
