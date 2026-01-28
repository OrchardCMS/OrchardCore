using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.DataLocalization.Services.Tests;

public class DataLocalizationProviderTests
{
    [Fact]
    public async Task ContentTypeDataLocalizationProvider_GetLocalizedStrings()
    {
        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager.Setup(cds => cds.ListTypeDefinitionsAsync())
            .ReturnsAsync(() => new List<ContentTypeDefinition> {
                new("Article", "Article"),
                new("BlogPost", "BlogPost"),
                new("News", "News"),
            });
        var dataLocalizationProvider = new ContentTypeDataLocalizationProvider(contentDefinitionManager.Object);
        var localizedStrings = await dataLocalizationProvider.GetDescriptorsAsync();

        Assert.Equal(3, localizedStrings.Count());
        Assert.True(localizedStrings.All(s => s.Context == "Content Types"));
    }

    [Fact]
    public async Task ContentFieldDataLocalizationProvider_GetLocalizedStrings()
    {
        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager.Setup(cds => cds.ListTypeDefinitionsAsync())
            .ReturnsAsync(() => new List<ContentTypeDefinition>
            {
                CreateContentTypeDefinition("BlogPost", "Blog Post", ["Title", "Body", "Author"]),
                CreateContentTypeDefinition("Person", "Person",  ["FirstName", "LastName"]),
            });
        var dataLocalizationProvider = new ContentFieldDataLocalizationProvider(contentDefinitionManager.Object);
        var localizedStrings = await dataLocalizationProvider.GetDescriptorsAsync();

        Assert.Equal(2, localizedStrings.Count());
        Assert.Equal("BlogPost.TextField", localizedStrings.ElementAt(0).Context);
        Assert.Equal("Person.TextField", localizedStrings.ElementAt(1).Context);
    }

    private static ContentTypeDefinition CreateContentTypeDefinition(string name, string displayName, string[] fields)
    {
        var contentPartFieldDefinitions = new List<ContentPartFieldDefinition>();
        var settings = new JsonObject();

        foreach (var field in fields)
        {
            contentPartFieldDefinitions.Add(new ContentPartFieldDefinition(new ContentFieldDefinition("TextField"), field, settings));
        }

        return new ContentTypeDefinition(
            name,
            displayName,
            new List<ContentTypePartDefinition>
            {
                new(name, new ContentPartDefinition(name, contentPartFieldDefinitions, settings), settings),
            },
            settings);
    }
}
