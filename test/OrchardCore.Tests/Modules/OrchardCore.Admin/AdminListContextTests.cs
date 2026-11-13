using OrchardCore.Admin;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListContextTests
{
    [Fact]
    public void Constructor_GenericContext_SharesSourceAndTypeWithNonGenericPipeline()
    {
        string[] items = ["item"];
        var context = new AdminListContext<string> { Source = items };
        AdminListContext untyped = context;

        Assert.Equal(typeof(string), untyped.ItemType);
        Assert.Same(items, context.Source);
        Assert.Same(items, untyped.Source);

        string[] replacement = [];
        context.Source = replacement;

        Assert.Same(replacement, untyped.Source);
    }
}
