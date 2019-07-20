using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes.Services
{
    public interface IStereotypeService
    {
        Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();
    }

    public class StereotypeService : IStereotypeService
    {
        private readonly IEnumerable<IStereotypesProvider> _providers;

        public StereotypeService(IEnumerable<IStereotypesProvider> providers)
        {
            _providers = providers;
        }

        public async Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
        {
            var stereotypes = new List<StereotypeDescription>();
            foreach (var provider in _providers)
            {
                stereotypes.AddRange(await provider.GetStereotypesAsync());
            }
            return stereotypes;
        }
    }
}
