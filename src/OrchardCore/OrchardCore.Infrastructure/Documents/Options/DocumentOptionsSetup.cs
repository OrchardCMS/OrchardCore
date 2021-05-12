using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptionsSetup : IConfigureNamedOptions<DocumentOptions>
    {
        private readonly IShellConfiguration _shellConfiguration;

        public DocumentOptionsSetup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public void Configure(DocumentOptions options) => Configure(String.Empty, options);

        public void Configure(string name, DocumentOptions options)
        {
            var config = _shellConfiguration.GetSection(name).Get<DocumentOptions>() ?? new DocumentOptions();

            config.CacheKey ??= name;
            config.CacheIdKey ??= "ID_" + name;
            config.CheckConcurrency ??= true;
            config.CheckConsistency ??= true;
            config.SynchronizationLatency ??= TimeSpan.FromSeconds(1);

            config.Serializer = DefaultDocumentSerializer.Instance;

            if (config.CompressThreshold == 0)
            {
                config.CompressThreshold = 10_000;
            }

            // Only used by an explicit atomic update.
            if (config.LockTimeout <= 0)
            {
                config.LockTimeout = 10_000;
            }

            if (config.LockExpiration <= 0)
            {
                config.LockExpiration = 10_000;
            }

            options.CacheKey = config.CacheKey;
            options.CacheIdKey = config.CacheIdKey;
            options.CheckConcurrency = config.CheckConcurrency;
            options.CheckConsistency = config.CheckConsistency;
            options.SynchronizationLatency = config.SynchronizationLatency;
            options.Serializer = config.Serializer;
            options.CompressThreshold = config.CompressThreshold;

            // Only used by an explicit atomic update.
            options.LockTimeout = config.LockTimeout;
            options.LockExpiration = config.LockExpiration;
        }
    }
}
