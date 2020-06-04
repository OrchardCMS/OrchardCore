using System.Threading.Tasks;

namespace OrchardCore.ShortCodes
{
    public interface IShortCode
    {
        ValueTask<string> ProcessAsync(string input);
    }
}
