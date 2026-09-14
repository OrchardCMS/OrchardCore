using OrchardCore.Navigation;

namespace OrchardCore.Tests.Navigation;

public class BreadcrumbBuilderTests
{
    [Fact]
    public void Add_WithItemBuilder_ConfiguresTheNode()
    {
        var builder = new BreadcrumbBuilder("Contents.Edit");

        builder.Add("Manage Content", item => item
            .Id("Contents")
            .Position("5")
            .AddClass("text-muted")
            .Action("List", "Admin", "OrchardCore.Contents"));

        var item = Assert.Single(builder.Items);

        Assert.Equal("Manage Content", item.Text);
        Assert.Equal("Contents", item.Id);
        Assert.Equal("5", item.Position);
        Assert.Equal(["text-muted"], item.Classes);
        Assert.Equal("List", item.RouteValues["action"]);
        Assert.Equal("Admin", item.RouteValues["controller"]);
        Assert.Equal("OrchardCore.Contents", item.RouteValues["area"]);
    }

    [Fact]
    public void Remove_WithMatchingNode_DropsItFromTheTrail()
    {
        var builder = new BreadcrumbBuilder("Contents.Edit");

        builder.Add("Manage Content", item => item.Id("Contents"));
        builder.Add("Edit Site Page", item => item.Id("ContentItem"));

        builder.Remove(item => item.Id == "Contents");

        Assert.Equal("Edit Site Page", Assert.Single(builder.Items).Text);
    }

    [Fact]
    public void TryGetData_WithAnotherType_ReturnsFalse()
    {
        var data = new Dictionary<string, object> { { "ContentType", "SitePage" } };
        var builder = new BreadcrumbBuilder("Contents.List", data);

        Assert.False(builder.TryGetData<int>("ContentType", out _));
        Assert.False(builder.TryGetData<string>("Missing", out _));
        Assert.True(builder.TryGetData<string>("ContentType", out var contentType));
        Assert.Equal("SitePage", contentType);
    }

    [Fact]
    public void GetData_WithoutData_ReturnsTheDefaultValue()
    {
        var builder = new BreadcrumbBuilder("Contents.List");

        Assert.Null(builder.GetData<string>("ContentType"));
    }

    [Fact]
    public void Constructor_WithoutName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new BreadcrumbBuilder(null));
        Assert.Throws<ArgumentException>(() => new BreadcrumbBuilder(string.Empty));
    }
}
