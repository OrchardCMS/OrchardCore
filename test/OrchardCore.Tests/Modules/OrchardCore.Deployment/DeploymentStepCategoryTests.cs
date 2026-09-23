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
    [InlineData(typeof(IndexProfileDeploymentStep), "Indexing")]
    [InlineData(typeof(AllContentDeploymentStep), "Content Management")]
    [InlineData(typeof(AllRolesDeploymentStep), "Security")]
    [InlineData(typeof(DeploymentPlanDeploymentStep), "Deployment")]
    public void Create_FromFactory_SetsUntranslatedCategory(Type stepType, string category)
    {
        // Act
        var step = CreateStep(stepType);

        // Assert
        Assert.Equal(category, step.Category.Name);
        Assert.Equal(category, step.Category.Value);
    }

    [Theory]
    [InlineData(typeof(IndexProfileDeploymentStep), "Indexing", "Indexation")]
    [InlineData(typeof(AllContentDeploymentStep), "Content Management", "Gestion du contenu")]
    public void Category_LocalizedWithStepType_UsesStepContext(Type stepType, string category, string translation)
    {
        // Arrange
        var step = CreateStep(stepType);
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord(category, stepType.FullName, [translation]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Create(step.GetType())[step.Category.Name];

        // Assert
        Assert.Equal(translation, localized.Value);
    }

    [Fact]
    public void Category_LocalizedWithStepType_MatchesTypedLocalizer()
    {
        // Arrange
        var step = CreateStep(typeof(IndexProfileDeploymentStep));
        var localizerFactory = CreateLocalizerFactory("fr", new CultureDictionaryRecord("Indexing", typeof(IndexProfileDeploymentStep).FullName, ["Indexation"]));

        // The removed constructor set the category with an IStringLocalizer<TStep>.
        var typedLocalizer = new StringLocalizer<IndexProfileDeploymentStep>(localizerFactory);

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Create(step.GetType())[step.Category.Name];

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
