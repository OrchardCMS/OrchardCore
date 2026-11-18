using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentTypes;

public class ContentTypeSettingsDisplayDriverTests
{
    private const string CustomUserSettings = "CustomUserSettings";

    [Theory]
    [InlineData(null, CustomUserSettings, false)]
    [InlineData(CustomUserSettings, null, true)]
    public async Task UpdateAsync_StereotypeChanges_OnlyUpdatesVisibleSettings(string storedStereotype, string postedStereotype, bool expectedSetting)
    {
        var definition = new ContentTypeDefinitionBuilder()
            .WithName("Profile")
            .Stereotype(storedStereotype)
            .Creatable()
            .Listable()
            .Draftable()
            .Versionable()
            .Securable()
            .Build();
        var options = new ContentTypeDefinitionOptions();
        options.Stereotypes[CustomUserSettings] = new ContentTypeDefinitionDriverOptions
        {
            ShowCreatable = false,
            ShowListable = false,
            ShowDraftable = false,
            ShowVersionable = false,
        };
        var driver = new ContentTypeSettingsDisplayDriver(
            Mock.Of<IStringLocalizer<ContentTypeSettingsDisplayDriver>>(),
            Options.Create(options),
            Mock.Of<IStereotypeService>(),
            Mock.Of<IContentDefinitionManager>());

        // Unchecked and absent checkboxes both bind to false.
        var updater = new Mock<IUpdateModel>();
        updater.Setup(u => u.TryUpdateModelAsync(It.IsAny<ContentTypeSettingsViewModel>(), It.IsAny<string>()))
            .Callback((ContentTypeSettingsViewModel model, string _) => model.Stereotype = postedStereotype)
            .ReturnsAsync(true);
        var builder = new ContentTypeDefinitionBuilder(definition);
        var context = new UpdateTypeEditorContext(builder, null, string.Empty, false, null, null, updater.Object);

        await driver.UpdateAsync(definition, context);

        var settings = builder.Build().GetSettings<ContentTypeSettings>();
        Assert.Equal(postedStereotype, settings.Stereotype);
        Assert.Equal(expectedSetting, settings.Creatable);
        Assert.Equal(expectedSetting, settings.Listable);
        Assert.Equal(expectedSetting, settings.Draftable);
        Assert.Equal(expectedSetting, settings.Versionable);
        Assert.False(settings.Securable);
    }
}
