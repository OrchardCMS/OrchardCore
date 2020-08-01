using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public interface IShortcodeTableProvider
    {
        Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync();
    }
}
