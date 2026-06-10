namespace OrchardCore.Modules.Services.Tests;

public class TimeZoneSelectListProviderTests
{
    [Fact]
    public void GetTimeZoneSelectListItems_MapsAndSortsTimeZones()
    {
        // Arrange
        var clock = new Mock<IClock>();
        clock
            .Setup(x => x.GetTimeZones())
            .Returns(
            [
                MockTimeZone("America/New_York", "(GMT-05) America/New_York"),
                MockTimeZone("Europe/Paris", "(GMT+01) Europe/Paris"),
            ]);

        var provider = new DefaultTimeZoneSelectListProvider(clock.Object);

        // Act
        var items = provider.GetTimeZoneSelectListItems();

        // Assert
        Assert.Collection(
            items,
            item =>
            {
                Assert.Equal("Europe/Paris", item.Value);
                Assert.Equal("(GMT+01) Europe/Paris", item.Text);
                Assert.False(item.Selected);
            },
            item =>
            {
                Assert.Equal("America/New_York", item.Value);
                Assert.Equal("(GMT-05) America/New_York", item.Text);
                Assert.False(item.Selected);
            });
    }

    private static ITimeZone MockTimeZone(string timeZoneId, string displayText)
    {
        var timeZone = new Mock<ITimeZone>();
        timeZone.SetupProperty(x => x.TimeZoneId, timeZoneId);
        timeZone.Setup(x => x.ToString()).Returns(displayText);

        return timeZone.Object;
    }
}
