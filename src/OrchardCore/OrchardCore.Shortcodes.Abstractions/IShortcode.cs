using System.Threading.Tasks;

namespace OrchardCore.Shortcodes
{
    public interface IShortcode
    {
        ValueTask<string> ProcessAsync(string input);
    }
}
