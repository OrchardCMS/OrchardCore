using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplay : IDependency
    {
        Task<DisplayResult<IContent>> BuildDisplayAsync(BuildDisplayContext<IContent> context);
        Task<DisplayResult<IContent>> BuildEditorAsync(BuildEditorContext<IContent> context);
        Task<DisplayResult<IContent>> UpdateEditorAsync(UpdateEditorContext<IContent> context, IUpdateModel updater);
    }
}
