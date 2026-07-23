using OrchardCore.Contents.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentFilterNodeTests
{
    [Fact]
    public void ContentTypeFilterNodeShouldUseExistingTermForMultipleValues()
    {
        var node = new ContentTypeFilterNode(["Article", "BlogPost"]);

        Assert.Equal("type", node.TermName);
        Assert.Equal("Article,BlogPost", node.Operation.ToString());
    }

    [Fact]
    public void StereotypeFilterNodeShouldUseExistingTermForMultipleValues()
    {
        var node = new StereotypeFilterNode(["Page", "Widget"]);

        Assert.Equal("stereotype", node.TermName);
        Assert.Equal("Page,Widget", node.Operation.ToString());
    }
}
