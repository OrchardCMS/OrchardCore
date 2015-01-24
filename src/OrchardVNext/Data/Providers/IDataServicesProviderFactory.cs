using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.Data.Providers {
    public interface IDataServicesProviderFactory : IDependency {
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