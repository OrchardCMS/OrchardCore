using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptionsFactory : IDocumentOptionsFactory
    {
        private static readonly string _baseFullName = typeof(Document).FullName;

        private readonly IShellConfiguration _shellConfiguration;

        private readonly ConcurrentDictionary<string, DocumentOptions> _cache = new ConcurrentDictionary<string, DocumentOptions>();

        public DocumentOptionsFactory(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public DocumentOptions Create(Type documentType)
        {
            return _cache.GetOrAdd(documentType.FullName, fullName =>
            {
                var section = _shellConfiguration.GetSection(fullName);

                if (!section.Exists())
                {
                    section = _shellConfiguration.GetSection(_baseFullName);
                }

                var options = section.Get<DocumentOptions>() ?? new DocumentOptions();

                options.CacheKey ??= fullName;
                options.CacheIdKey ??= "ID_" + fullName;
                options.CheckConcurrency ??= true;
                options.CheckConsistency ??= true;

                return options;
            });
        }
    }
}
