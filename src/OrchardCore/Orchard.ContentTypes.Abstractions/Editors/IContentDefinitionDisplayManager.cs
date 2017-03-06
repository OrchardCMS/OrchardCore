using System.Threading.Tasks;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentTypes.Editors
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