using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public interface IDisplayDriver
    {
        Task<IDisplayResult> BuildDisplayAsync(object model, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(object model, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(object model, UpdateEditorContext context);
    }
}
