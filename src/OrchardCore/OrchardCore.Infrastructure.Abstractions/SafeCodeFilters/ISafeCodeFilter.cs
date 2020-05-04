using System.Threading.Tasks;

namespace OrchardCore.Infrastructure.SafeCodeFilters
{
    public interface ISafeCodeFilter
    {
        ValueTask<string> ProcessAsync(string input);
    }
}
