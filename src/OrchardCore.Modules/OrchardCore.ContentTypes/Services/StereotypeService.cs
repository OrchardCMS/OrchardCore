using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes.Services
{
    public class StereotypeService : IStereotypeService
    {
        private readonly IEnumerable<IStereotypesProvider> _providers;

        public StereotypeService(IEnumerable<IStereotypesProvider> providers)
        {
            _providers = providers;
        }

        public async Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
        {
            var descriptions = new List<StereotypeDescription>();

            foreach (var provider in _providers)
            {
                descriptions.AddRange(await provider.GetStereotypesAsync());
            }

            return descriptions;
        }
    }
}
