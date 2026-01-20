using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public interface IShortcodeDescriptorManager
    {
        Task<IEnumerable<ShortcodeDescriptor>> GetShortcodeDescriptors();
    }
}
