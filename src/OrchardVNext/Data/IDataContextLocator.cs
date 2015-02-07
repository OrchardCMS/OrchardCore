using System;

namespace OrchardVNext.Data {
    public interface IDataContextLocator : IDependency {
        DataContext For(Type entityType);
    }
}