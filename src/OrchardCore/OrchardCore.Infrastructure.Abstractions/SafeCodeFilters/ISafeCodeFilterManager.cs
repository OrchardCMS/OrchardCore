using System.Threading.Tasks;

namespace OrchardCore.Infrastructure.SafeCodeFilters
{
    public interface ISafeCodeFilterManager
    {
        ValueTask<string> ProcessAsync(string input);
    }
}
