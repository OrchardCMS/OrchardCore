using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplayDriver : IDependency
    {
        Task<IDisplayResult> BuildDisplayAsync(string fieldName, ContentPart contentPart, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(string fieldName, ContentPart contentPart, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(string fieldName, ContentPart contentPart, UpdateEditorContext context);
    }
}
