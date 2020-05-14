using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Data
{
    public class SessionWrapper : ISession
    {
        private readonly ISession _session;

        private readonly HashSet<object> _saved = new HashSet<object>();

        private bool _canceled;

        public SessionWrapper(ISession session)
        {
            _session = session;

            ShellScope.RegisterBeforeDispose(scope => BeforeDisposeAsync());
        }

        public Task BeforeDisposeAsync()
        {
            if (_session == null)
            {
                return Task.CompletedTask;
            }

            // TODO: See what to do when 'CommitAsync()' is called explicitly as in 'UserStore'.

            if (!_canceled)
            {
                // A save followed by a query and then a mutation, needs another save to persist the last mutation.
                foreach (var item in _saved)
                {
                    _session.Save(item);
                }
            }
            else
            {
                // A cancel followed by a query, a mutation and then a save, needs another cancel to not persist the last mutation.
                _session.Cancel();
            }

            return _session.CommitAsync();
        }

        public IStore Store => _session.Store;

        public void Cancel()
        {
            _canceled = true;
            _session.Cancel();
        }

        public Task CommitAsync() => _session.CommitAsync();

        public void Delete(object item) => _session.Delete(item);

        public Task<DbTransaction> DemandAsync() => _session.DemandAsync();

        public void Detach(object item) => _session.Detach(item);

        public void Dispose() => _session.Dispose();

        public IQuery<T> ExecuteQuery<T>(ICompiledQuery<T> compiledQuery) where T : class => _session.ExecuteQuery<T>(compiledQuery);

        public Task FlushAsync() => _session.FlushAsync();

        public Task<IEnumerable<T>> GetAsync<T>(int[] ids) where T : class => _session.GetAsync<T>(ids);

        public bool Import(object item, int id) => _session.Import(item, id);

        public IQuery Query() => _session.Query();

        public ISession RegisterIndexes(params IIndexProvider[] indexProviders) => _session.RegisterIndexes(indexProviders);

        public void Save(object item, bool checkConcurrency = false)
        {
            _session.Save(item, checkConcurrency);
            _saved.Add(item);
        }
    }
}
