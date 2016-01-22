using Orchard.ContentManagement.Display.Handlers;
using Orchard.ContentManagement.Display.Views;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplay : IDependency
    {
        Task<DisplayResult> BuildDisplayAsync(BuildDisplayContext context);
        Task<DisplayResult> BuildEditorAsync(BuildEditorContext context);
        Task<DisplayResult> UpdateEditorAsync(UpdateEditorContext context, IUpdateModel updater);
    }
}
