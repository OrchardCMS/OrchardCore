using System.Threading.Tasks;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// Renders the edit and update shapes for the Site Settings object.
    /// </summary>
    public interface ISiteSettingsDisplayManager
    {
        Task<dynamic> BuildEditorAsync(IUpdateModel updater, string groupId);
        Task<dynamic> UpdateEditorAsync(IUpdateModel updater, string groupId);
    }
}
