using System.Text.Json;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Tests.DisplayManagement.Notify;

public class NotifierTests
{
    [Fact]
    public async Task SuccessAsync_OutMilliseconds_SetsDefaultMilliseconds()
    {
        var notifier = new Notifier(NullLogger<Notifier>.Instance);

        await notifier.SuccessAsync(new LocalizedHtmlString("Saved", "Saved"));

        var entry = Assert.Single(notifier.List());
        Assert.Equal(NotifyType.Success, entry.Type);
        Assert.Equal(5000, entry.DismissalMilliseconds);
    }

    [Fact]
    public async Task ErrorAsync_OutMilliseconds_DoesNotSetMilliseconds()
    {
        var notifier = new Notifier(NullLogger<Notifier>.Instance);

        await notifier.ErrorAsync(new LocalizedHtmlString("Problem", "Problem"));

        var entry = Assert.Single(notifier.List());
        Assert.Equal(NotifyType.Error, entry.Type);
        Assert.Null(entry.DismissalMilliseconds);
    }

    [Fact]
    public async Task SuccessAsync_Milliseconds_SetsMillisecondsOnNotifyEntry()
    {
        var notifier = new Notifier(NullLogger<Notifier>.Instance);

        await notifier.SuccessAsync(new LocalizedHtmlString("Saved", "Saved"), 3000);

        var entry = Assert.Single(notifier.List());
        Assert.Equal(NotifyType.Success, entry.Type);
        Assert.Equal(3000, entry.DismissalMilliseconds);
    }

    [Fact]
    public void NotifyEntryConverter_Default_RoundTripsMilliseconds()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NotifyEntryConverter(HtmlEncoder.Default));

        var entry = new NotifyEntry
        {
            Type = NotifyType.Warning,
            Message = new HtmlString("<strong>Heads up</strong>"),
            DismissalMilliseconds = 3000,
        };

        var json = JsonSerializer.Serialize(entry, options);
        var result = JsonSerializer.Deserialize<NotifyEntry>(json, options);

        Assert.Contains(@"""DismissalMilliseconds"":3000", json);
        Assert.NotNull(result);
        Assert.Equal(NotifyType.Warning, result.Type);
        Assert.Equal(3000, result.DismissalMilliseconds);
        Assert.Equal("<strong>Heads up</strong>", result.ToHtmlString(HtmlEncoder.Default));
    }
}
