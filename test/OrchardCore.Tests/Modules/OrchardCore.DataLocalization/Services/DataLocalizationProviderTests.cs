using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.DataLocalization.Services.Tests;

public class DataLocalizationProviderTests
{
    [Fact]
    public async Task ContentTypeDataLocalizationProvider_GetLocalizedStrings()
    {
        var contentDefinitionService = new Mock<IContentDefinitionViewModelService>();
        contentDefinitionService.Setup(cds => cds.GetTypesAsync())
            .ReturnsAsync(() => new List<EditTypeViewModel> {
                new() { DisplayName = "Article" },
                new() { DisplayName = "BlogPost" },
                new() { DisplayName = "News" },
            });
        var dataLocalizationProvider = new ContentTypeDataLocalizationProvider(contentDefinitionService.Object);
        var localizedStrings = await dataLocalizationProvider.GetDescriptorsAsync();

        Assert.Equal(3, localizedStrings.Count());
        Assert.True(localizedStrings.All(s => s.Context == "Content Types"));
    }

    [Fact]
    public async Task ContentFieldDataLocalizationProvider_GetLocalizedStrings()
    {
        var contentDefinitionService = new Mock<IContentDefinitionViewModelService>();
        contentDefinitionService.Setup(cds => cds.GetTypesAsync())
            .ReturnsAsync(() => new List<EditTypeViewModel>
            {
                new()
                {
                    Name = "BlogPost",
                    DisplayName = "Blog Post",
                    TypeDefinition = CreateContentTypeDefinition("BlogPost", "Blog Post", ["Title", "Body", "Author"]),
                },
                new()
                {
                    Name = "Person",
                    DisplayName = "Person",
                    TypeDefinition = CreateContentTypeDefinition("Person", "Person",  ["FirstName", "LastName"]),
                },
            });
        var dataLocalizationProvider = new ContentFieldDataLocalizationProvider(contentDefinitionService.Object);
        var localizedStrings = await dataLocalizationProvider.GetDescriptorsAsync();

        Assert.Equal(5, localizedStrings.Count());
        Assert.True(localizedStrings.All(s => s.Context == "Content Fields"));
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
