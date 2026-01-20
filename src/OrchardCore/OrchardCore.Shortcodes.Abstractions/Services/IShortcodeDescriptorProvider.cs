using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public interface IShortcodeDescriptorProvider
    {
        Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync();
    }
}
