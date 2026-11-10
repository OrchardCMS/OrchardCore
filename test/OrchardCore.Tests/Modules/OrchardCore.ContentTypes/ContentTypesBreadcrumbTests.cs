using AngleSharp.Html.Parser;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentTypes;

public class ContentTypesBreadcrumbTests
{
    private const string ReusablePartName = "BreadcrumbTestPart";
    private const string ReusablePartDisplayName = "Breadcrumb Test";

    [Fact]
    public async Task EditField_PartOfAContentType_LeadsBackThroughTheContentType()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var nodes = await GetTrailAsync(context, "Admin/ContentParts/Article/Fields/Subtitle/Edit", "\"Subtitle\" settings");

        Assert.Equal(["Content Types", "Edit Content Type - Article", "\"Subtitle\" settings"], nodes);
    }

    [Theory]
    [InlineData($"Admin/ContentParts/{ReusablePartName}/Fields/Text/Edit", "\"Text\" settings")]
    [InlineData($"Admin/ContentTypes/AddFieldsTo/{ReusablePartName}", $"Add New Field To \"{ReusablePartDisplayName}\"")]
    public async Task FieldPage_ReusablePart_LeadsBackThroughTheContentPart(string path, string title)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await contentDefinitionManager.AlterPartDefinitionAsync(ReusablePartName, part => part
                .WithDisplayName(ReusablePartDisplayName)
                .Attachable()
                .WithField("Text", field => field.OfType("TextField")));
        });

        var nodes = await GetTrailAsync(context, path, title);

        Assert.Equal(["Content Parts", $"Edit Content Part - {ReusablePartDisplayName}", title], nodes);
    }

    private static async Task<string[]> GetTrailAsync(SiteContext context, string path, string title)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = new HtmlParser().ParseDocument(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(title, Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title")).TextContent);

        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));

        return trail.QuerySelectorAll(".breadcrumb-item").Select(node => node.TextContent).ToArray();
    }
}
