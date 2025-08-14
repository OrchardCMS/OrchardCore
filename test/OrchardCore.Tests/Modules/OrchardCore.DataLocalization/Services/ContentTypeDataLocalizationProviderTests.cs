using OrchardCore.ContentTypes.Models;
using OrchardCore.ContentTypes.Services;

namespace OrchardCore.DataLocalization.Services.Tests;

public class ContentTypeDataLocalizationProviderTests
{
    //private readonly ContentTypeDataLocalizationProvider _contentTypeDataLocalizationProvider;

    public ContentTypeDataLocalizationProviderTests()
    {
        var contentDefinitionService = new Mock<IContentDefinitionService>();
        contentDefinitionService.Setup(service => service.GetTypesAsync())
            .ReturnsAsync(() => new List<EditType> {
                new() { DisplayName = "Article" },
                new() { DisplayName = "BlogPost" },
                new() { DisplayName = "News" },
            });

        //_contentTypeDataLocalizationProvider = new ContentTypeDataLocalizationProvider(contentDefinitionService.Object);
    }

    //[InlineData("Article", "es", "Artículo")]
    //[Theory]
    //public async Task LocalizeContentTypeNames(string contentType, string culture, string localizedName)
    //{
    //    await Task.CompletedTask;
    //}
}
