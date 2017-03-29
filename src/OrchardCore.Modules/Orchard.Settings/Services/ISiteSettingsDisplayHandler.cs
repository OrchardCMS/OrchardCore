using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.Settings.Services
{
    public interface ISiteSettingsDisplayHandler
    {
        Task BuildEditorAsync(ISite site, BuildEditorContext context);
        Task UpdateEditorAsync(ISite site, UpdateEditorContext context);
    }
}
