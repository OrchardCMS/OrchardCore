using OrchardCore.Contents.Deployment;
using OrchardCore.ContentTypes.Deployment;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;
using OrchardCore.Roles.Deployment;
using OrchardCore.Tests.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.Deployment;

public class DeploymentStepCategoryTests
{
    [Theory]
    [InlineData(typeof(IndexProfileDeploymentStep))]
    [InlineData(typeof(AllContentDeploymentStep))]
    [InlineData(typeof(DeploymentPlanDeploymentStep))]
    public void Create_MultipleInstances_SharesUntranslatedSources(Type stepType)
    {
        var factory = CreateFactory(stepType);

        Assert.NotSame(factory.Create(), factory.Create());
        Assert.Same(factory.Category, CreateFactory(stepType).Category);
        Assert.Same(factory.Title, CreateFactory(stepType).Title);
    }

    [Theory]
    [InlineData(typeof(IndexProfileDeploymentStep), "Indexing", "OrchardCore.Indexing.Core.IndexingLocalizationSources", "Index Profiles", "OrchardCore.Indexing.Core.IndexingLocalizationSources")]
    [InlineData(typeof(AllContentDeploymentStep), "Content Management", "OrchardCore.Localization.CommonLocalizationSources", "All Content", "OrchardCore.Contents.ContentsLocalizationSources")]
    [InlineData(typeof(DeploymentPlanDeploymentStep), "Deployment", "OrchardCore.Deployment.DeploymentLocalizationSources", "Deployment Plans", "OrchardCore.Deployment.DeploymentLocalizationSources")]
    public void Factory_Describes_UntranslatedCategoryAndTitle(Type stepType, string category, string categoryContext, string title, string titleContext)
    {
        // Act
        var factory = CreateFactory(stepType);

        // Assert
        Assert.Equal(category, factory.Category.Value);
        Assert.Equal(categoryContext, factory.Category.Type.FullName);
        Assert.Equal(title, factory.Title.Value);
        Assert.Equal(titleContext, factory.Title.Type.FullName);
    }

    [Fact]
    public void Create_StepsOfDifferentModules_ShareTheirCategory()
    {
        // Act
        var contentFactory = CreateFactory(typeof(AllContentDeploymentStep));
        var definitionFactory = CreateFactory(typeof(ContentDefinitionDeploymentStep));

        // Assert
        Assert.Same(CommonLocalizationSources.ContentManagement, contentFactory.Category);
        Assert.Same(contentFactory.Category, definitionFactory.Category);
    }

    [Theory]
    [InlineData(typeof(AllContentDeploymentStep), "Content Management", "Gestion du contenu")]
    [InlineData(typeof(AllRolesDeploymentStep), "Security", "Sécurité")]
    public void LocalizeCategory_TranslationInCommonContext_ReturnsTranslation(Type stepType, string category, string translation)
    {
        // Arrange
        var factory = CreateFactory(stepType);
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord(category, typeof(CommonLocalizationSources).FullName, [translation]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Localize(factory.Category);

        // Assert
        Assert.Equal(translation, localized.Value);
    }

    [Fact]
    public void LocalizeCategory_TranslationInModuleContext_ReturnsTranslation()
    {
        // Arrange
        var factory = CreateFactory(typeof(IndexProfileDeploymentStep));
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord("Indexing", "OrchardCore.Indexing.Core.IndexingLocalizationSources", ["Indexation"]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Localize(factory.Category);

        // Assert
        Assert.Equal("Indexation", localized.Value);
    }

    [Fact]
    public void LocalizeTitle_TranslationInModuleContext_ReturnsTranslation()
    {
        // Arrange
        var factory = CreateFactory(typeof(IndexProfileDeploymentStep));
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord("Index Profiles", "OrchardCore.Indexing.Core.IndexingLocalizationSources", ["Profils d'index"]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Localize(factory.Title);

        // Assert
        Assert.Equal("Profils d'index", localized.Value);
    }

    [Fact]
    public void LocalizeTitle_StepWithoutTitle_ReturnsNull()
    {
        // Arrange
        var factory = CreateFactory(typeof(AllRolesDeploymentStep));
        var localizerFactory = new Mock<IStringLocalizerFactory>();

        // Act
        var localized = localizerFactory.Object.Localize(factory.Title);

        // Assert
        Assert.Null(localized);
        localizerFactory.VerifyNoOtherCalls();
    }

    private static IDeploymentStepFactory CreateFactory(Type stepType)
    {
        var factoryType = typeof(DeploymentStepFactory<>).MakeGenericType(stepType);

        return (IDeploymentStepFactory)Activator.CreateInstance(factoryType, new ServiceCollection().BuildServiceProvider());
    }

    private static PortableObjectStringLocalizerFactory CreateLocalizerFactory(string cultureName, params CultureDictionaryRecord[] records)
    {
        var dictionary = new CultureDictionary(cultureName, PluralizationRule.English);
        dictionary.MergeTranslations(records);

        var localizationManager = new Mock<ILocalizationManager>();
        localizationManager
            .Setup(manager => manager.GetDictionary(It.Is<CultureInfo>(culture => culture.Name == cultureName)))
            .Returns(dictionary);

        return new PortableObjectStringLocalizerFactory(
            localizationManager.Object,
            Options.Create(new RequestLocalizationOptions()),
            NullLogger<PortableObjectStringLocalizerFactory>.Instance);
    }
}
