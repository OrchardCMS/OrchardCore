using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public interface IShortcodeTableProvider
    {
        Task<IEnumerable<ShortcodeDescriptor>> DescribeAsync();
    }
}
