using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement;

public class ZoneHoldingTests
{
    [Fact]
    public async Task ResetZones_PopulatedAndRenderedZones_PreservesConfiguration()
    {
        var factoryCalls = 0;
        var holder = new ZoneHolding<int>(state =>
        {
            factoryCalls += state;
            return ValueTask.FromResult<IShape>(new Shape());
        }, 1);
        holder.Metadata.Type = "Layout";
        holder.Metadata.Alternates.Add("Layout__Custom");
        holder.Classes.Add("custom");
        holder.Properties["Setting"] = "value";
        var model = new object();
        holder.Properties["Model"] = model;
        var zones = holder.Zones;

        await zones["Content"].AddAsync(new HtmlString("failed body"));
        await zones["Pending"].AddAsync(new Shape());
        zones["Footer"] = (IShape)PositionWrapper.TryWrap(new HtmlString("old widget"), "");
        zones["Explicit"] = new Shape();
        holder.Properties["Direct"] = new Shape();
        dynamic layout = holder;
        var placeholder = (IShape)layout.Empty;
        await layout.Dynamic.AddAsync(new HtmlString("dynamic content"));
        holder.Metadata.ChildContent = new HtmlString("old layout");

        holder.ResetZones();

        Assert.Same(zones, holder.Zones);
        Assert.Equal("Layout", holder.Metadata.Type);
        Assert.Equal("Layout__Custom", holder.Metadata.Alternates.Last);
        Assert.Contains("custom", holder.Classes);
        Assert.Equal("value", holder.Properties["Setting"]);
        Assert.Same(model, holder.Properties["Model"]);
        Assert.Null(holder.Metadata.ChildContent);
        Assert.False(zones.IsNotEmpty("Content"));
        Assert.False(zones.IsNotEmpty("Pending"));
        Assert.False(zones.IsNotEmpty("Footer"));
        Assert.False(zones.IsNotEmpty("Dynamic"));
        Assert.False(zones.IsNotEmpty("Explicit"));
        Assert.False(zones.IsNotEmpty("Direct"));
        Assert.False(holder.Properties.ContainsKey("Empty"));

        await zones["Content"].AddAsync(new HtmlString("error body"));
        Assert.Single(zones["Content"].Items);
        Assert.Equal(4, factoryCalls);
        Assert.NotSame(placeholder, (object)layout.Empty);
    }

    [Fact]
    public async Task ResetZones_CachedPlaceholder_AddsToCurrentZone()
    {
        var holder = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        var placeholder = holder.Zones["Content"];
        await placeholder.AddAsync(new HtmlString("failed body"));
        var originalZone = holder.Zones["Content"];

        holder.ResetZones();
        await holder.Zones["Content"].AddAsync(new HtmlString("error body"));
        await placeholder.AddAsync(new HtmlString("widget"));

        Assert.NotSame(originalZone, holder.Zones["Content"]);
        Assert.Single(originalZone.Items);
        Assert.Equal(2, holder.Zones["Content"].Items.Count);

        holder.ResetZones();
        await placeholder.AddAsync(new HtmlString("another body"));
        Assert.Single(holder.Zones["Content"].Items);
    }

    [Fact]
    public async Task ResetZones_DynamicZoneAndExplicitFactory_RecreatesZone()
    {
        var holder = new ZoneHolding(() => throw new InvalidOperationException("Wrong zone factory."));
        dynamic zones = new global::OrchardCore.DisplayManagement.Zones.Zones(() => ValueTask.FromResult<IShape>(new Shape()), holder);
        var placeholder = (IShape)zones.Header;
        await placeholder.AddAsync(new HtmlString("old"));

        holder.ResetZones();
        await placeholder.AddAsync(new HtmlString("new"));

        Assert.Single(holder.Zones["Header"].Items);
        Assert.Same(holder, holder.Zones["Header"].Properties["Parent"]);
        Assert.Equal("Header", holder.Zones["Header"].Properties["ZoneName"]);
    }
}
