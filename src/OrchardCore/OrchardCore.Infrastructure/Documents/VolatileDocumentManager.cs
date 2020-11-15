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
        private static readonly TimeSpan LockTimeout = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(1);

        private readonly IDistributedLock _distributedLock;
        private readonly string _lockKey;

        private delegate Task UpdateDelegate(TDocument document);
        private UpdateDelegate _updateDelegateAsync;

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

        public Task UpdateAtomicAsync(Func<TDocument, Task> updateAsync, Func<Task<TDocument>> factoryAsync = null)
        {
            if (updateAsync == null)
            {
                return Task.CompletedTask;
            }

            _updateDelegateAsync += document => updateAsync(document);

            _documentStore.AfterCommitSuccess<TDocument>(async () =>
            {
                (var locker, var locked) = await _distributedLock.TryAcquireLockAsync(_lockKey, LockTimeout, LockExpiration);
                if (!locked)
                {
                    return;
                }

                await using var acquiredLock = locker;
                var document = await GetOrCreateMutableAsync(factoryAsync);
                document.Identifier ??= IdGenerator.GenerateId();

                foreach (var d in _updateDelegateAsync.GetInvocationList())
                {
                    await ((UpdateDelegate)d)(document);
                }

                await SetInternalAsync(document);
            });

            return Task.CompletedTask;
        }
    }
}
