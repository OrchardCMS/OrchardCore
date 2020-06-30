using System.Threading.Tasks;

namespace OrchardCore.ShortCodes.Services
{
    public interface IShortCodeService
    {
        ValueTask<string> ProcessAsync(string input);
    }
}
