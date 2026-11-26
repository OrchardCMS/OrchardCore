using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Sitemaps.Controllers;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Tests.Sitemaps;

public class SourceControllerTests
{
    [Theory]
    [InlineData("contenttypessitemapsource")]
    [InlineData(nameof(ContentTypesSitemapSource))]
    public async Task Create_SourceTypeInAnyCase_SavesSelectedSourceType(string sourceType)
    {
        var sitemap = new Sitemap { SitemapId = "map" };
        var sitemapManager = new Mock<ISitemapManager>();
        sitemapManager.Setup(x => x.GetSitemapAsync(sitemap.SitemapId)).ReturnsAsync(sitemap);
        sitemapManager.Setup(x => x.LoadSitemapAsync(sitemap.SitemapId)).ReturnsAsync(sitemap);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var displayManager = new Mock<IDisplayManager<SitemapSource>>();
        displayManager.Setup(x => x.BuildEditorAsync(It.IsAny<SitemapSource>(), null, true, "", "")).ReturnsAsync(new Shape());
        displayManager.Setup(x => x.UpdateEditorAsync(It.IsAny<SitemapSource>(), null, true, "", "")).ReturnsAsync(new Shape());

        var idGenerator = new Mock<ISitemapIdGenerator>();
        idGenerator.Setup(x => x.GenerateUniqueId()).Returns("source");
        var htmlLocalizer = new Mock<IHtmlLocalizer<SourceController>>();
        htmlLocalizer.Setup(x => x[It.IsAny<string>()]).Returns((string name) => new LocalizedHtmlString(name, name));
        var controller = new SourceController(
            authorizationService.Object,
            displayManager.Object,
            [new SitemapSourceFactory<ContentTypesSitemapSource>(idGenerator.Object)],
            sitemapManager.Object,
            Mock.Of<IUpdateModelAccessor>(),
            Mock.Of<INotifier>(),
            Mock.Of<IStringLocalizer<SourceController>>(),
            htmlLocalizer.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        var view = Assert.IsType<ViewResult>(await controller.Create(sitemap.SitemapId, sourceType));
        var model = Assert.IsType<CreateSourceViewModel>(view.Model);
        Assert.IsType<ContentTypesSitemapSource>(model.SitemapSource);

        var result = await controller.Create(new CreateSourceViewModel
        {
            SitemapId = model.SitemapId,
            SitemapSourceId = model.SitemapSourceId,
            SitemapSourceType = model.SitemapSourceType,
        });

        Assert.IsType<RedirectToActionResult>(result);
        var source = Assert.IsType<ContentTypesSitemapSource>(Assert.Single(sitemap.SitemapSources));
        Assert.Equal("source", source.Id);
        sitemapManager.Verify(x => x.UpdateSitemapAsync(sitemap), Times.Once);
    }
}
