using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ShortCodes.Services
{
    public class ShortCodeService : IShortCodeService
    {
        private readonly IEnumerable<IShortCode> _shortCodes;

        public ShortCodeService(IEnumerable<IShortCode> shortCodes)
        {
            _shortCodes = shortCodes;
        }

        public async ValueTask<string> ProcessAsync(string input)
        {
            foreach (var shortCode in _shortCodes)
            {
                input = await shortCode.ProcessAsync(input);
            }

            return input;
        }
    }
}
