using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptionsSetup : IConfigureNamedOptions<DocumentOptions>
    {
        private readonly IShellConfiguration _shellConfiguration;

        private readonly ConcurrentDictionary<string, DocumentOptions> _cache = new ConcurrentDictionary<string, DocumentOptions>();

        public DocumentOptionsSetup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public void Configure(DocumentOptions options) => Configure(String.Empty, options);

        public void Configure(string name, DocumentOptions options)
        {
            var config = _cache.GetOrAdd(name, name =>
            {
                var options = _shellConfiguration.GetSection(name).Get<DocumentOptions>() ?? new DocumentOptions();

                options.CacheKey ??= name;
                options.CacheIdKey ??= "ID_" + name;
                options.CheckConcurrency ??= true;
                options.CheckConsistency ??= true;
                options.SynchronizationLatency ??= TimeSpan.FromSeconds(1);

                options.Serializer = DefaultDocumentSerializer.Instance;

                if (options.CompressThreshold == 0)
                {
                    options.CompressThreshold = 10_000;
                }

                return options;
            });

            options.CacheKey = config.CacheKey;
            options.CacheIdKey = config.CacheIdKey;
            options.CheckConcurrency = config.CheckConcurrency;
            options.CheckConsistency = config.CheckConsistency;
            options.SynchronizationLatency = config.SynchronizationLatency;
            options.Serializer = config.Serializer;
            options.CompressThreshold = config.CompressThreshold;
        }
    }
}
