using OrchardVNext.Environment.Configuration;
using System;

namespace OrchardVNext.Data
{
    public interface ITransactionManager : IDependency {
        void Demand();
        void RequireNew();
        void Cancel();
    }

    public class TransactionManager : ITransactionManager, IDisposable {
        private readonly ShellSettings _shellSettings;
        private DataContext _dataContext;
        private bool _isCancel;

        public TransactionManager(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public void Demand() {
            _dataContext = new DataContext(_shellSettings);
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