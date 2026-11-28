using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using OrchardCore.DataLocalization.Controllers;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.DataLocalization.ViewModels;
using OrchardCore.Documents;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Controllers.Tests;

public sealed class AdminControllerTests
{
    private const string Context = "Tabs and Cards";

    [Fact]
    public async Task GetStrings_StringOfSeveralProviders_ListsItOnce()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = Assert.IsType<OkObjectResult>(await controller.GetStrings("el-GR"));

        // Assert
        var groups = (IList<TranslatableStringGroupViewModel>)result.Value.GetType().GetProperty("providers").GetValue(result.Value);
        var strings = Assert.Single(groups).Strings;
        Assert.Equal(["Settings", "Content", "Metadata"], strings.Select(value => value.Key));
        Assert.Equal("Ρυθμίσεις", strings[0].Value);
    }

    [Fact]
    public async Task GetStatisticsJson_StringOfSeveralProviders_CountsItOnce()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = Assert.IsType<OkObjectResult>(await controller.GetStatisticsJson());

        // Assert
        var culture = Assert.Single(Assert.IsType<TranslationStatisticsViewModel>(result.Value).ByCulture);
        Assert.Equal(3, culture.Total);
        Assert.Equal(1, culture.Translated);
    }

    /// <summary>
    /// Creates a controller whose two providers both list "Settings", in another case, and whose translations store
    /// translates it.
    /// </summary>
    private static AdminController CreateController()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var localizationService = Mock.Of<ILocalizationService>(service => service.GetSupportedCulturesAsync() == Task.FromResult(new[] { "el-GR" }));

        var document = new TranslationsDocument();
        document.Translations["el-GR"] = [new Translation { Context = Context, Key = "Settings", Value = "Ρυθμίσεις" }];

        var documentManager = new Mock<IDocumentManager<TranslationsDocument>>();
        documentManager
            .Setup(manager => manager.GetOrCreateImmutableAsync(It.IsAny<Func<Task<TranslationsDocument>>>()))
            .ReturnsAsync(document);

        var controller = new AdminController(
            authorizationService.Object,
            localizationService,
            [CreateProvider("Settings", "Content"), CreateProvider("settings", "Metadata")],
            new TranslationsManager(documentManager.Object, new MemoryCache(new MemoryCacheOptions())),
            Mock.Of<IStringLocalizer<AdminController>>());

        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        return controller;
    }

    private static ILocalizationDataProvider CreateProvider(params string[] names)
    {
        IEnumerable<DataLocalizedString> descriptors = names.Select(name => new DataLocalizedString(Context, name, string.Empty)).ToArray();

        return Mock.Of<ILocalizationDataProvider>(provider => provider.GetDescriptorsAsync() == Task.FromResult(descriptors));
    }
}
