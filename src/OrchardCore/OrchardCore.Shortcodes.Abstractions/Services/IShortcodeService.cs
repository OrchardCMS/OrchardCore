using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public interface IShortcodeService
    {
        ValueTask<string> ProcessAsync(string input);
    }
}
