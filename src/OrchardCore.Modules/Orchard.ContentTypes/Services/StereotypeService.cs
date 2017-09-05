using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentTypes.Services
{
    public interface IStereotypeService
    {
        IEnumerable<StereotypeDescription> GetStereotypes();
    }

    public class StereotypeService : IStereotypeService
    {
        private readonly IEnumerable<IStereotypesProvider> _providers;

        public StereotypeService(IEnumerable<IStereotypesProvider> providers)
        {
            _providers = providers;
        }

        public IEnumerable<StereotypeDescription> GetStereotypes()
        {
            return _providers.SelectMany(x => x.GetStereotypes());
        }
    }
}