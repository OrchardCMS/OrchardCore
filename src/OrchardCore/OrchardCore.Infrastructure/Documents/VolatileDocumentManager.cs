using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Data.Documents;
using OrchardCore.Documents.Options;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="DocumentManager{TDocument}"/> using a multi level cache but without any persistent storage.
    /// </summary>
    public class VolatileDocumentManager<TDocument> : DocumentManager<TDocument>, IVolatileDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        private const string LockKeySuffix = "_LOCK";
        private static readonly TimeSpan LockTimeout = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(1);

        private readonly IDistributedLock _distributedLock;
        private readonly string _lockKey;

        private delegate Task<TDocument> UpdateDelegate();
        private UpdateDelegate _updateDelegateAsync;

        public VolatileDocumentManager(
            IDocumentStore documentStore,
            IDocumentSerialiser<TDocument> serializer,
            IDistributedCache distributedCache,
            IDistributedLock distributedLock,
            IMemoryCache memoryCache,
            DocumentOptions<TDocument> options)
            : base(documentStore, serializer, distributedCache, memoryCache, options)
        {
            _isVolatile = true;

            _distributedLock = distributedLock;
            _lockKey = options.Value.CacheKey + LockKeySuffix;
        }

        public Task UpdateAtomicAsync(Func<Task<TDocument>> updateAsync)
        {
            if (updateAsync == null)
            {
                return Task.CompletedTask;
            }

            _updateDelegateAsync += () => updateAsync();

            _documentStore.AfterCommitSuccess<TDocument>(async () =>
            {
                (var locker, var locked) = await _distributedLock.TryAcquireLockAsync(_lockKey, LockTimeout, LockExpiration);
                if (!locked)
                {
                    return;
                }

                await using var acquiredLock = locker;
                var updated = await _updateDelegateAsync();
                updated.Identifier ??= IdGenerator.GenerateId();
                await SetInternalAsync(updated);
            });

            return Task.CompletedTask;
        }
    }
}
