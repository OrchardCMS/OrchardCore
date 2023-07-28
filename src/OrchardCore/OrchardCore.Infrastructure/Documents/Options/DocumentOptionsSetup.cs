using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptionsSetup : IConfigureNamedOptions<DocumentOptions>
    {
        public static readonly TimeSpan DefaultFailoverRetryLatency = TimeSpan.FromSeconds(30);

        private readonly IShellConfiguration _shellConfiguration;

        public DocumentOptionsSetup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public void Configure(DocumentOptions options) => Configure(String.Empty, options);

        public void Configure(string name, DocumentOptions options)
        {
            var sharedConfig = _shellConfiguration
                .GetSection("OrchardCore_Documents")
                .Get<DocumentSharedOptions>()
                ?? new DocumentSharedOptions();

            var namedConfig = _shellConfiguration
                .GetSection(name)
                .Get<DocumentNamedOptions>()
                ?? new DocumentNamedOptions();

            // Only from the named config or default.
            options.CacheKey = namedConfig.CacheKey ?? name;
            options.CacheIdKey = namedConfig.CacheIdKey ?? "ID_" + name;

            // Only from the shared config or default.
            options.FailoverRetryLatency = sharedConfig.FailoverRetryLatency ?? DefaultFailoverRetryLatency;

            // From the named or shared config or default.
            options.CheckConcurrency = namedConfig.CheckConcurrency ?? sharedConfig.CheckConcurrency ?? true;
            options.CheckConsistency = namedConfig.CheckConsistency ?? sharedConfig.CheckConsistency ?? true;

            options.SynchronizationLatency = namedConfig.SynchronizationLatency
                ?? sharedConfig.SynchronizationLatency
                ?? TimeSpan.FromSeconds(1);

            options.Serializer = DefaultDocumentSerializer.Instance;

            options.CompressThreshold = namedConfig.CompressThreshold;
            if (options.CompressThreshold == 0)
            {
                options.CompressThreshold = sharedConfig.CompressThreshold;
                if (options.CompressThreshold == 0)
                {
                    options.CompressThreshold = 10_000;
                }
            }

            // Only used by an explicit atomic update.
            options.LockTimeout = namedConfig.LockTimeout;
            if (options.LockTimeout <= 0)
            {
                options.LockTimeout = sharedConfig.LockTimeout;
                if (options.LockTimeout <= 0)
                {
                    options.LockTimeout = 10_000;
                }
            }

            // Only used by an explicit atomic update.
            options.LockExpiration = namedConfig.LockExpiration;
            if (options.LockExpiration <= 0)
            {
                options.LockExpiration = sharedConfig.LockExpiration;
                if (options.LockExpiration <= 0)
                {
                    options.LockExpiration = 10_000;
                }
            }

            // Inherited 'DistributedCacheEntryOptions'.
            options.AbsoluteExpiration = namedConfig.AbsoluteExpiration ?? sharedConfig.AbsoluteExpiration;
            options.AbsoluteExpirationRelativeToNow = namedConfig.AbsoluteExpirationRelativeToNow ?? sharedConfig.AbsoluteExpirationRelativeToNow;
            options.SlidingExpiration = namedConfig.SlidingExpiration ?? sharedConfig.SlidingExpiration;
        }
    }
}
