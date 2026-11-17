using System;
using Xunit;

namespace OrchardCore;

public class TextHelperTests
{
    [Theory]
    [InlineData(null, 3, null)]
    [InlineData("", 0, "")]
    [InlineData("abc", 0, "")]
    [InlineData("abc", 2, "ab")]
    [InlineData("abc", 3, "abc")]
    [InlineData("abc", 4, "abc")]
    [InlineData("a😀b", 2, "a")]
    [InlineData("a😀b", 3, "a😀")]
    [InlineData("ae\u0301b", 2, "a")]
    [InlineData("ae\u0301b", 3, "ae\u0301")]
    [InlineData("a🇺🇸b", 3, "a")]
    [InlineData("a🇺🇸b", 5, "a🇺🇸")]
    [InlineData("a👍🏽b", 3, "a")]
    [InlineData("a👩‍💻b", 5, "a")]
    [InlineData("a👩‍💻b", 6, "a👩‍💻")]
    [InlineData("a\r\nb", 2, "a")]
    public void Truncate_LengthLimit_PreservesWholeTextElements(string value, int limit, string expected)
    {
        Assert.Equal(expected, TextHelper.Truncate(value, limit));

        if (value is not null)
        {
            var length = TextHelper.GetTruncationLength(value.AsSpan(), limit);
            Assert.Equal(expected, value[..length]);
            Assert.InRange(length, 0, limit);
        }
    }

    [Fact]
    public void Truncate_NegativeLimit_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TextHelper.Truncate("text", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextHelper.GetTruncationLength("text".AsSpan(), -1));
    }
}
