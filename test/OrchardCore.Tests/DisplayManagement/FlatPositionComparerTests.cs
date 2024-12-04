using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement;

public class FlatPositionComparerTests
{
    [Theory]
    [InlineData("before", null)]
    [InlineData("10", "20")]
    [InlineData("before", "after")]
    [InlineData("before", "10")]
    [InlineData("10", "after")]
    [InlineData("after.10", "after.20")]
    public void FirstMustComeBeforeSecond(string first, string second)
    {
        var result = FlatPositionComparer.Instance
            .Compare(first, second);

        Assert.Equal(-1, result);
    }

    [Theory]
    [InlineData(null, "before")]
    [InlineData("20", "10")]
    [InlineData("after", "before")]
    [InlineData("10", "before")]
    [InlineData("after", "10")]
    [InlineData("after.20", "after.10")]
    public void SecondMustComeBeforeFirst(string first, string second)
    {
        var result = FlatPositionComparer.Instance
            .Compare(first, second);

        Assert.Equal(1, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("after", "after")]
    [InlineData("before", "before")]
    [InlineData("10", "10")]
    public void FirstPositionEqualsSecond(string first, string second)
    {
        var result = FlatPositionComparer.Instance
            .Compare(first, second);

        Assert.Equal(0, result);
    }
}

