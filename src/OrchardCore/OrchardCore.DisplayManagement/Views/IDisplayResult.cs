using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.DisplayManagement.Views
{
    public interface IDisplayResult
    {
        Task ApplyAsync(BuildDisplayContext context);
        Task ApplyAsync(BuildEditorContext context);
    }
}
