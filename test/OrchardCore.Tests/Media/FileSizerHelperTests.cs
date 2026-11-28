using OrchardCore.Media.Core.Helpers;

namespace OrchardCore.Tests.Media;

public class FileSizeHelperTests
{
    private readonly Mock<IStringLocalizer<FileSizeHelper>> _stringLocalizerMock;

    public FileSizeHelperTests()
    {
        _stringLocalizerMock = new Mock<IStringLocalizer<FileSizeHelper>>();
        _stringLocalizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _stringLocalizerMock.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, string.Format(key, args)));
    }

    [Fact]
    public void ZeroBytes_Returns_0_KB()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        // Act
        var result = fileSizeHelper.FormatSize(0);

        // Assert
        Assert.Equal("0 KB", result);
    }

    [Fact]
    public void NegativeBytes_Prefixes_Minus()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        // Act
        var result = fileSizeHelper.FormatSize(-1024);

        // Assert
        Assert.Equal("-1 KB", result);
    }

    [Theory]
    [InlineData(1, "1 B")]
    [InlineData(500, "500 B")]
    [InlineData(1000, "1000 B")]
    [InlineData(1.4, "1 B")]
    [InlineData(1.5, "2 B")]
    [InlineData(1023.4, "1023 B")]
    public void BytesLessThan1KB_Uses_B_Unit(long bytes, string expected)
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        // Act & Assert
        Assert.Equal(expected, fileSizeHelper.FormatSize(bytes));
    }

    [Fact]
    public void ExactKB_MB_GB_TB_PB_AreFormattedWithTheirUnits()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        const long KB = 1024L;
        const long MB = KB * KB;
        const long GB = MB * KB;
        const long TB = GB * KB;
        const long PB = TB * KB;

        // Act & Assert
        Assert.Equal("1 KB", fileSizeHelper.FormatSize(KB));
        Assert.Equal("1 MB", fileSizeHelper.FormatSize(MB));
        Assert.Equal("1 GB", fileSizeHelper.FormatSize(GB));
        Assert.Equal("1 TB", fileSizeHelper.FormatSize(TB));
        Assert.Equal("1 PB", fileSizeHelper.FormatSize(PB));
    }

    [Fact]
    public void LongMaxValue_Formats_As_EB_With_AdjustedSize()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);
        var bytes = long.MaxValue;
        var magnitude = (int)Math.Log(bytes, 1024);
        var adjustedSize = bytes / Math.Pow(1024, magnitude);
        var unit = magnitude switch
        {
            0 => "B",
            1 => "KB",
            2 => "MB",
            3 => "GB",
            4 => "TB",
            5 => "PB",
            _ => "EB"
        };
        var expected = $"{Math.Round(adjustedSize, 2).ToString(CultureInfo.CurrentCulture)} {unit}";

        // Act
        var result = fileSizeHelper.FormatSize(bytes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DigitsParameter_IsHandledCorrectly()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        // Act & Assert
        Assert.Equal("2 KB", fileSizeHelper.FormatSize(1536, digits: 0));
        Assert.Equal($"{Math.Round(1536d / 1024, 5).ToString(CultureInfo.CurrentCulture)} KB", fileSizeHelper.FormatSize(1536, digits: 5));
    }
}
