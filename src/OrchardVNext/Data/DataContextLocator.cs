using System;
using Microsoft.AspNet.Mvc;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Data {
    public class DataContextLocator : IDataContextLocator, ITransactionManager, IDisposable {
        private readonly ShellSettings _shellSettings;
        private readonly IDbContextFactoryHolder _dbContextFactoryHolder;
        private readonly IServiceProvider _serviceProvider;

        private DataContext _dataContext;
        private bool _isCancel;

        public DataContextLocator(ShellSettings shellSettings,
            IDbContextFactoryHolder dbContextFactoryHolder,
            IServiceProvider serviceProvider) {
            _shellSettings = shellSettings;
            _dbContextFactoryHolder = dbContextFactoryHolder;
            _serviceProvider = serviceProvider;
        }

        public DataContext For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);

            Demand();
            
            return _dataContext;
        }

        public void Demand() {
            _dataContext = new DataContext(
                _shellSettings, 
                _dbContextFactoryHolder.BuildConfiguration(),
                (IAssemblyProvider)_serviceProvider.GetService(typeof(IAssemblyProvider)));
        }

        public void RequireNew() {
            _dataContext.SaveChanges();
            _dataContext.Dispose();
            Demand();
        }

        public void Cancel() {
            _isCancel = true;
            _dataContext.Dispose();
        }

        public void Dispose() {
            if (!_isCancel) {
                _dataContext.SaveChanges();
                _dataContext.Dispose();
            }
        }
    }
}