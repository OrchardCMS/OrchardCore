using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.DataLocalization.Services.Tests;

public class ContentTypeDataLocalizationProviderTests
{
    //private readonly ContentTypeDataLocalizationProvider _contentTypeDataLocalizationProvider;

    public ContentTypeDataLocalizationProviderTests()
    {
        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager.Setup(service => service.ListTypeDefinitionsAsync())
            .ReturnsAsync(() => new List<ContentTypeDefinition> {
                new("Article", "Article"),
                new("BlogPost", "BlogPost"),
                new("News", "News"),
            });

        //_contentTypeDataLocalizationProvider = new ContentTypeDataLocalizationProvider(contentDefinitionManager.Object);
    }

    //[InlineData("Article", "es", "Artículo")]
    //[Theory]
    //public async Task LocalizeContentTypeNames(string contentType, string culture, string localizedName)
    //{
    //    await Task.CompletedTask;
    //}
}
