using OrchardCore.Contents.Deployment;
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
    [InlineData(typeof(IndexProfileDeploymentStep), "Indexing", "Index Profiles")]
    [InlineData(typeof(AllContentDeploymentStep), "Content Management", "All Content")]
    [InlineData(typeof(DeploymentPlanDeploymentStep), "Deployment", "Deployment Plans")]
    public void Create_FromFactory_SetsUntranslatedCategoryAndTitle(Type stepType, string category, string title)
    {
        // Act
        var step = CreateStep(stepType);

        // Assert
        Assert.Equal(category, step.Category.Name);
        Assert.Equal(category, step.Category.Value);
        Assert.Equal(title, step.Title.Name);
        Assert.Equal(title, step.Title.Value);
    }

    [Theory]
    [InlineData(typeof(IndexProfileDeploymentStep), "Indexing", "Indexation")]
    [InlineData(typeof(AllContentDeploymentStep), "Content Management", "Gestion du contenu")]
    public void GetLocalizedCategory_TranslationInStepContext_ReturnsTranslation(Type stepType, string category, string translation)
    {
        // Arrange
        var step = CreateStep(stepType);
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord(category, stepType.FullName, [translation]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = step.GetLocalizedCategory(localizerFactory);

        // Assert
        Assert.Equal(translation, localized.Value);
    }

    [Fact]
    public void GetLocalizedTitle_TranslationInStepContext_ReturnsTranslation()
    {
        // Arrange
        var step = CreateStep(typeof(IndexProfileDeploymentStep));
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord("Index Profiles", typeof(IndexProfileDeploymentStep).FullName, ["Profils d'index"]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = step.GetLocalizedTitle(localizerFactory);

        // Assert
        Assert.Equal("Profils d'index", localized.Value);
    }

    [Fact]
    public void GetLocalizedTitle_StepWithoutTitle_ReturnsNull()
    {
        // Arrange
        var step = CreateStep(typeof(AllRolesDeploymentStep));
        var localizerFactory = new Mock<IStringLocalizerFactory>();

        // Act
        var localized = step.GetLocalizedTitle(localizerFactory.Object);

        // Assert
        Assert.Null(localized);
        localizerFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetLocalizedCategory_TranslationInStepContext_MatchesTypedLocalizer()
    {
        // Arrange
        var step = CreateStep(typeof(IndexProfileDeploymentStep));
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord("Indexing", typeof(IndexProfileDeploymentStep).FullName, ["Indexation"]));

        // The removed constructor set the category with an IStringLocalizer<TStep>.
        var typedLocalizer = new StringLocalizer<IndexProfileDeploymentStep>(localizerFactory);

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = step.GetLocalizedCategory(localizerFactory);

        // Assert
        Assert.Equal(typedLocalizer["Indexing"].Value, localized.Value);
    }

    private static DeploymentStep CreateStep(Type stepType)
    {
        var factoryType = typeof(DeploymentStepFactory<>).MakeGenericType(stepType);
        var factory = (IDeploymentStepFactory)Activator.CreateInstance(factoryType, new ServiceCollection().BuildServiceProvider());

        return factory.Create();
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
