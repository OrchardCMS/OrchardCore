using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentDisplayHandler : IDependency
    {
        Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context);
        Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context);
        Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context);
    }
}
