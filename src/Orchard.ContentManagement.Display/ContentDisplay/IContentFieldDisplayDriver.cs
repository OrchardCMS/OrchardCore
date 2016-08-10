using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplayDriver : IDependency
    {
        Task<IDisplayResult> BuildDisplayAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context);
    }
}
