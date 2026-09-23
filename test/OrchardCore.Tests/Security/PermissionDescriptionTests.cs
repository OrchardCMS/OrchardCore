using OrchardCore.Contents;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Localization;

namespace OrchardCore.Tests.Security;

public class PermissionDescriptionTests
{
    [Fact]
    public void Constructor_WithStringDescription_CreatesDescriptionWithoutContext()
    {
        // Act
        var permission = new Permission("ManageThings", "Manage things");

        // Assert
        Assert.Equal("Manage things", permission.Description.Name);
        Assert.Equal("Manage things", permission.Description.Value);
        Assert.Null(permission.Description.SearchedLocation);
    }

    [Fact]
    public void Constructor_WithNullStringDescription_HasNoDescription()
    {
        // Act
        var permission = new Permission("ManageThings", (string)null);

        // Assert
        Assert.Null(permission.Description);
    }

    [Fact]
    public void Constructor_WithLocalizedDescription_KeepsContext()
    {
        // Arrange
        var impliedBy = new Permission("ManageAllThings");

        // Act
        var permission = new Permission(
            "ManageThings",
            LocalizedString.Create("Manage things", typeof(PermissionDescriptionTests)),
            [impliedBy],
            isSecurityCritical: true);

        // Assert
        Assert.Equal("Manage things", permission.Description.Name);
        Assert.Equal(typeof(PermissionDescriptionTests).FullName, permission.Description.SearchedLocation);
        Assert.Same(impliedBy, Assert.Single(permission.ImpliedBy));
        Assert.True(permission.IsSecurityCritical);
    }

    [Fact]
    public void Description_OfBuiltInPermission_UsesDeclaringTypeAsContext()
    {
        // Act
        var description = CommonPermissions.EditContent.Description;

        // Assert
        Assert.Equal("Edit content for others", description.Name);
        Assert.Equal(typeof(CommonPermissions).FullName, description.SearchedLocation);
    }

    [Fact]
    public void Localize_BuiltInPermissionDescription_UsesPoTranslationForDeclaringType()
    {
        // Arrange
        var localizerFactory = CreateLocalizerFactory(
            "fr",
            new CultureDictionaryRecord("Edit content for others", typeof(CommonPermissions).FullName, ["Modifier le contenu des autres"]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Localize(CommonPermissions.EditContent.Description);

        // Assert
        Assert.Equal("Modifier le contenu des autres", localized.Value);
        Assert.False(localized.ResourceNotFound);
    }

    [Fact]
    public void Localize_TranslationInOtherContext_ReturnsName()
    {
        // Arrange
        var localizerFactory = CreateLocalizerFactory(
            "fr",
            new CultureDictionaryRecord("Edit content for others", "Some.Other.Context", ["Modifier le contenu des autres"]));

        using var scope = CultureScope.Create("fr");

        // Act
        var localized = localizerFactory.Localize(CommonPermissions.EditContent.Description);

        // Assert
        Assert.Equal("Edit content for others", localized.Value);
        Assert.True(localized.ResourceNotFound);
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
