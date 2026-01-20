using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement.Zones;

public class FlatPositionComparerTests
{
    private readonly FlatPositionComparer _comparer = FlatPositionComparer.Instance;

    #region Test Helper Classes

    private sealed class TestPositioned : IPositioned
    {
        public string Position { get; set; }
    }

    #endregion

    #region Instance Tests

    [Fact]
    public void Instance_ShouldReturnSameInstance()
    {
        // Arrange & Act
        var instance1 = FlatPositionComparer.Instance;
        var instance2 = FlatPositionComparer.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    #endregion

    #region String Comparison Tests

    [Theory]
    [InlineData("1", "2", -1)]
    [InlineData("2", "1", 1)]
    [InlineData("1", "1", 0)]
    [InlineData("10", "2", 1)]
    [InlineData("2", "10", -1)]
    public void Compare_SimpleNumbers_ShouldCompareNumerically(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.1", "1.2", -1)]
    [InlineData("1.2", "1.1", 1)]
    [InlineData("1.1", "1.1", 0)]
    [InlineData("1.10", "1.2", 1)]
    [InlineData("1.2", "1.10", -1)]
    public void Compare_DecimalPositions_ShouldCompareNumerically(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.1.1", "1.1.2", -1)]
    [InlineData("1.1.2", "1.1.1", 1)]
    [InlineData("1.2.1", "1.1.2", 1)]
    [InlineData("1.1.2", "1.2.1", -1)]
    [InlineData("1.1.1", "1.1.1", 0)]
    public void Compare_MultiLevelPositions_ShouldCompareCorrectly(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1", "1.1", -1)]
    [InlineData("1.1", "1", 1)]
    [InlineData("1.2", "1.2.1", -1)]
    [InlineData("1.2.1", "1.2", 1)]
    public void Compare_DifferentLevels_ShouldCompareCorrectly(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, 0)]
    [InlineData(null, "1", -1)]
    [InlineData("1", null, 1)]
    [InlineData(null, "before", 1)]
    [InlineData("before", null, -1)]
    public void Compare_NullValues_ShouldHandleCorrectly(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "", 0)]
    [InlineData("", "1", -1)]
    [InlineData("1", "", 1)]
    [InlineData("", "0", 0)]
    [InlineData("0", "", 0)]
    [InlineData("   ", "0", 0)]
    [InlineData("0", "   ", 0)]
    public void Compare_EmptyValues_ShouldTreatAsZero(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("before", "after", -1)]
    [InlineData("after", "before", 1)]
    [InlineData("before", "before", 0)]
    [InlineData("after", "after", 0)]
    [InlineData("before", "1", -1)]
    [InlineData("1", "before", 1)]
    [InlineData("after", "1", 1)]
    [InlineData("1", "after", -1)]
    public void Compare_KnownPartitions_ShouldNormalizeCorrectly(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("BEFORE", "after", -1)]
    [InlineData("After", "BEFORE", 1)]
    [InlineData("BeFoRe", "AfTeR", -1)]
    public void Compare_KnownPartitions_ShouldBeCaseInsensitive(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("alpha", "beta", -1)]
    [InlineData("beta", "alpha", 1)]
    [InlineData("alpha", "alpha", 0)]
    [InlineData("Alpha", "alpha", 0)]
    [InlineData("ALPHA", "alpha", 0)]
    public void Compare_NonNumericStrings_ShouldCompareAlphabetically(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1:test", "2:test", -1)]
    [InlineData("2:test", "1:test", 1)]
    [InlineData("test:", "test", 0)]
    public void Compare_ColonSeparators_ShouldHandleCorrectly(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.2:3", "1.2:4", -1)]
    [InlineData("1:2.3", "1:2.4", -1)]
    [InlineData("1:2:3", "1:2:4", -1)]
    public void Compare_MixedSeparators_ShouldHandleCorrectly(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.", "1", 0)]
    [InlineData("1..", "1", 0)]
    [InlineData("test.", "test", 0)]
    [InlineData("test..", "test", 0)]
    public void Compare_TrailingDots_ShouldBeTrimmed(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Compare_SameReference_ShouldReturnZero()
    {
        // Arrange
        var position = "1.2.3";

        // Act
        var result = _comparer.Compare(position, position);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region IPositioned Comparison Tests

    [Fact]
    public void Compare_IPositioned_ShouldCompareByPosition()
    {
        // Arrange
        var item1 = new TestPositioned { Position = "1" };
        var item2 = new TestPositioned { Position = "2" };

        // Act
        var result = _comparer.Compare(item1, item2);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Compare_IPositioned_WithNullPositions_ShouldHandleCorrectly()
    {
        // Arrange
        var item1 = new TestPositioned { Position = null };
        var item2 = new TestPositioned { Position = "1" };

        // Act
        var result = _comparer.Compare(item1, item2);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Compare_IPositioned_SamePositions_ShouldReturnZero()
    {
        // Arrange
        var item1 = new TestPositioned { Position = "1.2" };
        var item2 = new TestPositioned { Position = "1.2" };

        // Act
        var result = _comparer.Compare(item1, item2);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region Complex Scenario Tests

    [Fact]
    public void Compare_BasicOrderingRules_ShouldWork()
    {
        // Test that basic ordering works for key transitions
        Assert.True(_comparer.Compare("before", "1") < 0, "before should come before 1");
        Assert.True(_comparer.Compare("1", "1.1") < 0, "1 should come before 1.1");
        Assert.True(_comparer.Compare("1.1", "1.2") < 0, "1.1 should come before 1.2");
        Assert.True(_comparer.Compare("1.2", "1.10") < 0, "1.2 should come before 1.10");
        Assert.True(_comparer.Compare("2", "10") < 0, "2 should come before 10");
        Assert.True(_comparer.Compare("10", "after") < 0, "10 should come before after");
    }

    [Theory]
    [InlineData("1.before", "1.1", -1)]
    [InlineData("1.after", "1.1", 1)]
    [InlineData("before.1", "1.before", -1)]
    [InlineData("after.1", "1.after", 1)]
    public void Compare_BeforeAfterInComplexPositions_ShouldWork(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("text1", "text2", -1)]
    [InlineData("text2", "text1", 1)]
    [InlineData("text1.1", "text1.2", -1)]
    [InlineData("text1.alpha", "text1.beta", -1)]
    public void Compare_TextBasedPositions_ShouldCompareAlphabetically(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Compare_MixedNumericAndText_ShouldHandleCorrectly()
    {
        // Test ordering between numbers and strings
        Assert.True(_comparer.Compare("1", "alpha") < 0, "Numbers should come before text");
        Assert.True(_comparer.Compare("2", "beta") < 0, "Numbers should come before text");
        Assert.True(_comparer.Compare("10", "gamma") < 0, "Numbers should come before text");
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("0", "00", 0)]
    [InlineData("1", "01", 0)]
    [InlineData("10", "010", 0)]
    public void Compare_LeadingZeros_ShouldBeNumericallyEqual(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.0", "1", 1)]
    [InlineData("1", "1.0", -1)]
    [InlineData("1.0.0", "1.0", 1)]
    [InlineData("1.0", "1.0.0", -1)]
    public void Compare_TrailingZeros_ShouldConsiderMoreSpecific(string x, string y, int expected)
    {
        // Act
        var result = _comparer.Compare(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("short", "verylongpositionname")]
    [InlineData("verylongpositionname", "short")]
    public void Compare_VeryLongStrings_ShouldCompareCorrectly(string x, string y)
    {
        // Act
        var result = _comparer.Compare(x, y);
        var expectedSign = string.Compare(x, y, StringComparison.OrdinalIgnoreCase);

        // Assert - Check that the sign is correct
        if (expectedSign < 0)
        {
            Assert.True(result < 0, $"{x} should come before {y}");
        }
        else if (expectedSign > 0)
        {
            Assert.True(result > 0, $"{x} should come after {y}");
        }
        else
        {
            Assert.Equal(0, result);
        }
    }

    [Theory]
    [InlineData("specialchars!@#", "specialchars$%^")]
    [InlineData("unicode-ñ", "unicode-ö")]
    public void Compare_SpecialCharacters_ShouldCompareAlphabetically(string x, string y)
    {
        // Act
        var result = _comparer.Compare(x, y);
        var expectedSign = string.Compare(x, y, StringComparison.OrdinalIgnoreCase);

        // Assert - Check that the sign is correct
        if (expectedSign < 0)
        {
            Assert.True(result < 0, $"{x} should come before {y}");
        }
        else if (expectedSign > 0)
        {
            Assert.True(result > 0, $"{x} should come after {y}");
        }
        else
        {
            Assert.Equal(0, result);
        }
    }

    #endregion

    #region Normalization Tests

    [Fact]
    public void Compare_BeforePartition_ShouldBeNormalized()
    {
        // Test that "before" gets normalized to a low value
        Assert.True(_comparer.Compare("before", "0") < 0);
        Assert.True(_comparer.Compare("BEFORE", "0") < 0);
        
        // Test that "before" is consistent
        Assert.Equal(0, _comparer.Compare("before", "BEFORE"));
    }

    [Fact]
    public void Compare_AfterPartition_ShouldBeNormalized()
    {
        // Test that "after" gets normalized to a high value
        Assert.True(_comparer.Compare("after", "0") > 0);
        Assert.True(_comparer.Compare("AFTER", "0") > 0);
        Assert.True(_comparer.Compare("after", "999") > 0);
        
        // Test that "after" is consistent
        Assert.Equal(0, _comparer.Compare("after", "AFTER"));
    }

    [Fact]
    public void Compare_NonNormalizedPartitions_ShouldNotBeSpecial()
    {
        // Test that similar but different strings don't get normalized
        var result1 = _comparer.Compare("beforex", "0");
        var result2 = _comparer.Compare("afterx", "0");
        
        // These should be string comparisons, not special values
        // beforex comes after "0" alphabetically, afterx comes after "0" alphabetically
        Assert.True(result1 > 0);
        Assert.True(result2 > 0);
    }

    [Fact]
    public void Compare_EmptyStringBehavior_ShouldBeTreatedAsZero()
    {
        // Empty string should be treated as "0"
        Assert.Equal(0, _comparer.Compare("", "0"));
        Assert.Equal(0, _comparer.Compare("0", ""));
        
        // Whitespace should also be treated as "0" 
        Assert.Equal(0, _comparer.Compare("   ", "0"));
        Assert.Equal(0, _comparer.Compare("0", "   "));
    }

    #endregion
}
