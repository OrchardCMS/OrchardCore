using OrchardCore.Media.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaCommandsTests
{
    [Fact]
    public void GetValues_OnNewInstance_IsEmpty()
    {
        var commands = new MediaCommands();

        var values = commands.GetValues().ToList();

        Assert.Empty(values);
    }

    [Fact]
    public void IndividualSetters_Then_GetValues_ReturnsExpectedPairs_InSortedOrder()
    {
        var commands = new MediaCommands();

        // set in an arbitrary order
        commands.Format = "jpg";
        commands.Width = "100";
        commands.Quality = "80";
        commands.ResizeMode = "crop";
        commands.BackgroundColor = "#ffffff";
        commands.ResizeFocalPoint = "0.5,0.5";
        commands.Height = "50";

        var list = commands.GetValues().ToList();

        // Expect order by the internal indices: Width(0), Height(1), ResizeMode(2), ResizeFocalPoint(3), Format(4), BackgroundColor(5), Quality(6)
        Assert.Equal(7, list.Count);

        Assert.Collection(list,
            item => { Assert.Equal(MediaCommands.WidthCommand, item.Key); Assert.Equal("100", item.Value); },
            item => { Assert.Equal(MediaCommands.HeightCommand, item.Key); Assert.Equal("50", item.Value); },
            item => { Assert.Equal(MediaCommands.ResizeModeCommand, item.Key); Assert.Equal("crop", item.Value); },
            item => { Assert.Equal(MediaCommands.ResizeFocalPointCommand, item.Key); Assert.Equal("0.5,0.5", item.Value); },
            item => { Assert.Equal(MediaCommands.FormatCommand, item.Key); Assert.Equal("jpg", item.Value); },
            item => { Assert.Equal(MediaCommands.BackgroundColorCommand, item.Key); Assert.Equal("#ffffff", item.Value); },
            item => { Assert.Equal(MediaCommands.QualityCommand, item.Key); Assert.Equal("80", item.Value); }
        );
    }

    [Fact]
    public void SetCommands_WithShuffledInput_IgnoresUnknowns_AndReturnsSorted()
    {
        var commands = new MediaCommands();

        var input = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("quality", "60"),
            new KeyValuePair<string, string>("unknown", "ignored"),
            new KeyValuePair<string, string>("width", "200"),
            new KeyValuePair<string, string>("format", "png"),
            new KeyValuePair<string, string>("height", "150"),
        };

        // apply commands in this shuffled order
        commands.SetCommands(input);

        var list = commands.GetValues().ToList();

        // Expect only known commands in canonical order: width, height, format, quality
        Assert.Equal(4, list.Count);

        Assert.Collection(list,
            item => { Assert.Equal(MediaCommands.WidthCommand, item.Key); Assert.Equal("200", item.Value); },
            item => { Assert.Equal(MediaCommands.HeightCommand, item.Key); Assert.Equal("150", item.Value); },
            item => { Assert.Equal(MediaCommands.FormatCommand, item.Key); Assert.Equal("png", item.Value); },
            item => { Assert.Equal(MediaCommands.QualityCommand, item.Key); Assert.Equal("60", item.Value); }
        );

        // ensure unknown command is not present
        Assert.DoesNotContain(list, kvp => kvp.Key == "unknown");
    }

    [Fact]
    public void SettingSameCommandTwice_OverwritesPreviousValue()
    {
        var commands = new MediaCommands();

        commands.Width = "100";
        commands.Width = "300"; // overwrite

        var list = commands.GetValues().ToList();

        Assert.Single(list);
        Assert.Equal(MediaCommands.WidthCommand, list[0].Key);
        Assert.Equal("300", list[0].Value);
    }

    [Fact]
    public void SettingCommandToNull_RemovesValue()
    {
        var commands = new MediaCommands();

        commands.Width = "100";

        var list = commands.GetValues().ToList();

        Assert.Single(list);

        commands.Width = null; // remove
        list = commands.GetValues().ToList();
        Assert.Empty(list);

    }

}
