using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public interface IShortcodeService
    {
        ValueTask<string> ProcessAsync(string input, Context context = null);
    }
}
