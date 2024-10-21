using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentTypes.Editors;

public interface IContentDefinitionDisplayManager
{
    [Obsolete("This method is marked obsolete and will be removed in future release.")]
    Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");

    [Obsolete("This method is marked obsolete and will be removed in future release.")]

    Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");

    Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, bool isNew, string groupId = "")
#pragma warning disable CS0618 // Type or member is obsolete
        => BuildTypeEditorAsync(definition, updater, groupId);
#pragma warning restore CS0618 // Type or member is obsolete

    Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, bool isNew, string groupId = "")
#pragma warning disable CS0618 // Type or member is obsolete
        => UpdateTypeEditorAsync(definition, updater, groupId);
#pragma warning restore CS0618 // Type or member is obsolete

    Task<dynamic> BuildPartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");
    Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");

    Task<dynamic> BuildTypePartEditorAsync(ContentTypePartDefinition definition, IUpdateModel updater, string groupId = "");
    Task<dynamic> UpdateTypePartEditorAsync(ContentTypePartDefinition definition, IUpdateModel updater, string groupId = "");

    Task<dynamic> BuildPartFieldEditorAsync(ContentPartFieldDefinition definition, IUpdateModel updater, string groupId = "");
    Task<dynamic> UpdatePartFieldEditorAsync(ContentPartFieldDefinition definition, IUpdateModel updater, string groupId = "");
}
