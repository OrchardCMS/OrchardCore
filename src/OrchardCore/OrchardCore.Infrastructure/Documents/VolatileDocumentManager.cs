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
        private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DefaultLockExpiration = TimeSpan.FromSeconds(1);

        private readonly IDistributedLock _distributedLock;
        private readonly string _lockKey;

        private delegate Task<TDocument> UpdateDelegate();
        private UpdateDelegate _updateDelegateAsync;

        private delegate Task AfterUpdateDelegate(TDocument document);
        private AfterUpdateDelegate _afterUpdateDelegateAsync;

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

        public Task UpdateAtomicAsync(Func<Task<TDocument>> updateAsync, Func<TDocument, Task> afterUpdateAsync = null,
            TimeSpan? lockAcquireTimeout = null, TimeSpan? lockExpirationTime = null)
        {
            if (updateAsync == null)
            {
                return Task.CompletedTask;
            }

            _updateDelegateAsync += () => updateAsync();

            if (afterUpdateAsync != null)
            {
                _afterUpdateDelegateAsync += document => afterUpdateAsync(document);
            }

            _documentStore.AfterCommitSuccess<TDocument>(async () =>
            {
                var timeout = lockAcquireTimeout ?? DefaultLockTimeout;
                var expiration = lockExpirationTime ?? DefaultLockExpiration;

                (var locker, var locked) = await _distributedLock.TryAcquireLockAsync(_lockKey, timeout, expiration);
                if (!locked)
                {
                    return;
                }

                await using var acquiredLock = locker;

                TDocument document = null;
                foreach (var d in _updateDelegateAsync.GetInvocationList())
                {
                    document = await ((UpdateDelegate)d)();
                }

                document.Identifier ??= IdGenerator.GenerateId();

                await SetInternalAsync(document);

                if (_afterUpdateDelegateAsync != null)
                {
                    foreach (var d in _afterUpdateDelegateAsync.GetInvocationList())
                    {
                        await ((AfterUpdateDelegate)d)(document);
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
