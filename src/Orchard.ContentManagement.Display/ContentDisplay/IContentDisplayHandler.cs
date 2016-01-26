using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentDisplayHandler : IDependency
    {
        Task BuildDisplayAsync(object model, BuildDisplayContext context);
        Task BuildEditorAsync(object model, BuildEditorContext context);
        Task UpdateEditorAsync(object model, UpdateEditorContext context);
    }
}
