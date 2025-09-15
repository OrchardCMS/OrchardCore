using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.DataLocalization.Services.Tests;

public class ContentTypeDataLocalizationProviderTests
{
    //private readonly ContentTypeDataLocalizationProvider _contentTypeDataLocalizationProvider;

    public ContentTypeDataLocalizationProviderTests()
    {
        var contentDefinitionService = new Mock<IContentDefinitionViewModelService>();
        contentDefinitionService.Setup(service => service.GetTypesAsync())
            .ReturnsAsync(() => new List<EditTypeViewModel> {
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
