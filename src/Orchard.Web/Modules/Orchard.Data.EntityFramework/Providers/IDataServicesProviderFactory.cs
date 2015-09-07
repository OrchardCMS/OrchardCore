using System.Collections.Generic;
using System.Linq;

namespace Orchard.Data.EntityFramework.Providers {
    public interface IDataServicesProviderFactory {
        IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters);
    }

    public class DataServicesProviderFactory : IDataServicesProviderFactory {
        private readonly IEnumerable<IDataServicesProvider> _dataServicesProviders;
        public DataServicesProviderFactory(IEnumerable<IDataServicesProvider> dataServicesProviders) {
            _dataServicesProviders = dataServicesProviders;
        }

        public IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters) {
            return _dataServicesProviders.First(x => x.ProviderName == sessionFactoryParameters.Provider);
        }
    }
}