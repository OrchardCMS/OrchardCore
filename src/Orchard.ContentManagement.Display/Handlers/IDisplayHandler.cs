using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.Handlers
{
    public interface IDisplayHandler : IDependency
    {
        Task BuildDisplayAsync(BuildDisplayContext context);
        Task BuildEditorAsync(BuildEditorContext context);
        Task UpdateEditorAsync(UpdateEditorContext context, IModelUpdater modelUpdater);
    }
}
