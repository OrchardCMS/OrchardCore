using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentTypes.Editors
{
    public interface IContentDefinitionDisplayManager
    {
        Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");

        Task<dynamic> BuildPartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");

        Task<dynamic> BuildTypePartEditorAsync(ContentTypePartDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateTypePartEditorAsync(ContentTypePartDefinition definition, IUpdateModel updater, string groupId = "");

        Task<dynamic> BuildPartFieldEditorAsync(ContentPartFieldDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdatePartFieldEditorAsync(ContentPartFieldDefinition definition, IUpdateModel updater, string groupId = "");
    }
}
