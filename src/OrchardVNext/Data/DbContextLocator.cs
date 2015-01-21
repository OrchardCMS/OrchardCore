using System;
using Microsoft.Data.Entity;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Data {
    public class DbContextLocator : IDbContextLocator, ITransactionManager, IDisposable {
        private readonly ShellSettings _shellSettings;
        private readonly IDbContextFactoryHolder _dbContextFactoryHolder;

        private DataContext _dataContext;
        private bool _isCancel;

        public DbContextLocator(ShellSettings shellSettings,
            IDbContextFactoryHolder dbContextFactoryHolder) {
            _shellSettings = shellSettings;
            _dbContextFactoryHolder = dbContextFactoryHolder;
        }

        public DbContext For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);

            Demand();

            return _dataContext.Context;
        }

        public void Demand() {
            _dataContext = new DataContext(_shellSettings, _dbContextFactoryHolder.BuildConfiguration());
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