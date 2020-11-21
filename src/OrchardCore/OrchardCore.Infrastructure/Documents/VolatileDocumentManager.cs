using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Documents;
using OrchardCore.Documents.Options;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="DocumentManager{TDocument}"/> using a multi level cache but without any persistent storage.
    /// </summary>
    public class VolatileDocumentManager<TDocument> : DocumentManager<TDocument>, IVolatileDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        private const string LockKeySuffix = "_LOCK";
        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(3);

        private readonly IDistributedLock _distributedLock;
        private readonly string _lockKey;

        private delegate Task<TDocument> UpdatingDelegate();
        private delegate Task UpdatedDelegate(TDocument document);
        private UpdatingDelegate _updatingDelegateAsync;
        private UpdatedDelegate _updatedDelegateAsync;

        public VolatileDocumentManager(
            IDocumentStore documentStore,
            IDistributedCache distributedCache,
            IDistributedLock distributedLock,
            IMemoryCache memoryCache,
            IOptionsSnapshot<DocumentOptions> options)
            : base(documentStore, distributedCache, memoryCache, options)
        {
            _isVolatile = true;
            _distributedLock = distributedLock;
            _lockKey = _options.CacheKey + LockKeySuffix;
        }

        public Task UpdateAsync(Func<Task<TDocument>> updatingAsync, Func<TDocument, Task> updatedAsync = null)
        {
            if (updatingAsync == null)
            {
                return Task.CompletedTask;
            }

            _updatingDelegateAsync += () => updatingAsync();

            if (updatedAsync != null)
            {
                _updatedDelegateAsync += document => updatedAsync(document);
            }

            _documentStore.AfterCommitSuccess<TDocument>(async () =>
            {
                TDocument document = null;
                foreach (var d in _updatingDelegateAsync.GetInvocationList())
                {
                    document = await ((UpdatingDelegate)d)();
                }

                document.Identifier ??= IdGenerator.GenerateId();

                await SetInternalAsync(document);

                if (_updatedDelegateAsync != null)
                {
                    foreach (var d in _updatedDelegateAsync.GetInvocationList())
                    {
                        await ((UpdatedDelegate)d)(document);
                    }
                }
            });

            return Task.CompletedTask;
        }

        public Task UpdateAtomicAsync(Func<Task<TDocument>> updatingAsync, Func<TDocument, Task> updatedAsync = null)
        {
            if (updatingAsync == null)
            {
                return Task.CompletedTask;
            }

            _updatingDelegateAsync += () => updatingAsync();

            if (updatedAsync != null)
            {
                _updatedDelegateAsync += document => updatedAsync(document);
            }

            _documentStore.AfterCommitSuccess<TDocument>(async () =>
            {
                (var locker, var locked) = await _distributedLock.TryAcquireLockAsync(_lockKey, LockTimeout, LockExpiration);
                if (!locked)
                {
                    return;
                }

                await using var acquiredLock = locker;

                TDocument document = null;
                foreach (var d in _updatingDelegateAsync.GetInvocationList())
                {
                    document = await ((UpdatingDelegate)d)();
                }

                document.Identifier ??= IdGenerator.GenerateId();

                await SetInternalAsync(document);

                if (_updatedDelegateAsync != null)
                {
                    foreach (var d in _updatedDelegateAsync.GetInvocationList())
                    {
                        await ((UpdatedDelegate)d)(document);
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
