namespace OrchardCore.Modules.Services.Tests;

public class TimeZoneSelectListProviderTests
{
    [Fact]
    public async Task GetTimeZoneSelectList_MapsAndSortsTimeZones_Succeeds()
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
        var items = await provider.GetTimeZoneSelectListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Collection(
            items,
            item =>
            {
                Assert.Equal("Europe/Paris", item.Key);
                Assert.Equal("(GMT+01) Europe/Paris", item.Value);
            },
            item =>
            {
                Assert.Equal("America/New_York", item.Key);
                Assert.Equal("(GMT-05) America/New_York", item.Value);
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
