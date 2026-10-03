using OrchardCore.Contents;
using OrchardCore.Contents.Security;
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
        Assert.Equal("Manage things", permission.Description.Value);
        Assert.Null(permission.Description.Type);
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
            LocalizationSource.Create<PermissionDescriptionTests>("Manage things"),
            [impliedBy],
            isSecurityCritical: true);

        // Assert
        Assert.Equal("Manage things", permission.Description.Value);
        Assert.Equal(typeof(PermissionDescriptionTests), permission.Description.Type);
        Assert.Same(impliedBy, Assert.Single(permission.ImpliedBy));
        Assert.True(permission.IsSecurityCritical);
    }

    [Fact]
    public void Description_OfBuiltInPermission_UsesDeclaringTypeAsContext()
    {
        // Act
        var description = CommonPermissions.EditContent.Description;

        // Assert
        Assert.Equal("Edit content for others", description.Value);
        Assert.Equal(typeof(CommonPermissions), description.Type);
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

    [Fact]
    public void CreateDynamicPermission_FromTemplate_KeepsContextFreeFormattedDescription()
    {
        var template = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.EditContent);

        var permission = ContentTypePermissionsHelper.CreateDynamicPermission(template, "Widget");

        Assert.Equal("Edit_Widget", permission.Name);
        Assert.Equal("Edit Widget for others", permission.Description.Value);
        Assert.Null(permission.Description.Type);
        Assert.NotEmpty(permission.ImpliedBy);
    }

    [Fact]
    public void Localize_NestedSourceType_UsesTypedLocalizerContext()
    {
        var source = LocalizationSource.Create<Messages>("Hello");
        var localizerFactory = CreateLocalizerFactory(
            "fr",
            new CultureDictionaryRecord("Hello", typeof(Messages).FullName.Replace('+', '.'), ["Bonjour"]));

        using var scope = CultureScope.Create("fr");

        var localized = localizerFactory.Localize(source);

        Assert.Equal("Bonjour", localized.Value);
        Assert.False(localized.ResourceNotFound);
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

    private sealed class Messages;
}
