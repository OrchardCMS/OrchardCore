using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplayDriver : IDependency
    {
        Task<IDisplayResult> BuildDisplayAsync(string fieldName, ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(string fieldName, ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(string fieldName, ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, UpdateEditorContext context);
    }
}
