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
        Assert.Equal("-1 B", result);
    }

    [Fact]
    public void BytesLessThan1KB_Uses_B_Unit()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        // Act & Assert
        Assert.Equal("1 B", fileSizeHelper.FormatSize(1));
        Assert.Equal("1.5 B", fileSizeHelper.FormatSize(1536));
        Assert.Equal("2 B", fileSizeHelper.FormatSize(2048));
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
        Assert.Equal("1 KB", fileSizeHelper.FormatSize(MB));
        Assert.Equal("1 MB", fileSizeHelper.FormatSize(GB));
        Assert.Equal("1 GB", fileSizeHelper.FormatSize(TB));
        Assert.Equal("1 TB", fileSizeHelper.FormatSize(PB));
        Assert.Equal("1 PB", fileSizeHelper.FormatSize(PB * KB));
    }

    [Fact]
    public void LongMaxValue_Formats_As_PB_With_AdjustedSize()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);
        var bytes = long.MaxValue;
        var magnitude = (int)Math.Log(bytes, 1024);
        var adjustedSize = bytes / Math.Pow(1024, magnitude);
        var unit = magnitude switch
        {
            0 or 1 => "B",
            2 => "KB",
            3 => "MB",
            4 => "GB",
            5 => "TB",
            6 => "PB",
            _ => "EB"
        };
        var expected = string.Format(CultureInfo.InvariantCulture, "{0} " + unit, adjustedSize);

        // Act
        var result = fileSizeHelper.FormatSize(bytes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DecimalPlacesParameter_IsIgnored_ByCurrentImplementation()
    {
        // Arrange
        var fileSizeHelper = new FileSizeHelper(_stringLocalizerMock.Object);

        // Act & Assert
        var a = fileSizeHelper.FormatSize(1536, decimalPlaces: 0);
        var b = fileSizeHelper.FormatSize(1536, decimalPlaces: 5);

        Assert.Equal(a, b);
    }
}
