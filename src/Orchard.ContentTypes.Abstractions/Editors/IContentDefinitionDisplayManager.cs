using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentTypes.Editors
{
    public interface IContentDefinitionDisplayManager : IDependency
    {
        Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");

        Task<dynamic> BuildPartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");
    }

}