using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Roles.Services
{
    public class CachedLookupNormalizer : ILookupNormalizer
    {
        private ImmutableDictionary<string, string> _cache = ImmutableDictionary.Create<string, string>();
        private UpperInvariantLookupNormalizer _normalizer = new UpperInvariantLookupNormalizer();
        private DateTime _cachedUtc = DateTime.UtcNow;
        private static readonly TimeSpan _cacheDelay = TimeSpan.FromHours(1);

        public string NormalizeEmail(string email)
        {
            return NormalizeName(email);
        }

        public string NormalizeName(string name)
        {
            // Invalidate the cache after some delay
            if (DateTime.UtcNow - _cachedUtc > _cacheDelay)
            {
                _cache = ImmutableDictionary.Create<string, string>();
            }

            if (!_cache.TryGetValue(name, out var result))
            {
                result = _normalizer.NormalizeEmail(name);
                _cache = _cache.Add(name, result);
            }

            return result;
        }
    }
}
