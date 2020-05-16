using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Infrastructure.SafeCodeFilters
{
    // Where shall we put this?
    // Project -> ShortCode
    public class DefaultSafeCodeFilterManager : ISafeCodeFilterManager
    {
        private readonly IEnumerable<ISafeCodeFilter> _safeCodeFilters;

        public DefaultSafeCodeFilterManager(IEnumerable<ISafeCodeFilter> safeCodeFilters)
        {
            _safeCodeFilters = safeCodeFilters;
        }

        public async ValueTask<string> ProcessAsync(string input)
        {
            foreach(var safeCodeFilter in _safeCodeFilters)
            {
                input = await safeCodeFilter.ProcessAsync(input);
            }

            return input;
        }
    }
}
