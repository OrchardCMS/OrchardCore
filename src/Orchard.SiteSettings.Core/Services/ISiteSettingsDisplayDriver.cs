using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// And implementation of <see cref="ISiteSettingsDisplayDriver"/> is called 
    /// every time the site settings editors are rendered, to return a shape that will
    /// be part of the full editor.
    /// </summary>
    public interface ISiteSettingsDisplayDriver
    {
        Task<IDisplayResult> BuildEditorAsync(ISite site, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(ISite site, UpdateEditorContext context);
    }
}
