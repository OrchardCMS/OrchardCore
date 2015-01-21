using System;

namespace OrchardVNext.Data.Providers {
    public interface IDataServicesProviderFactory : IDependency {
        IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters);
    }

}