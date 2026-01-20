using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Cache
{
    // Todo: does this belong in dynamic cache?
    public class CacheScopeManager : ICacheScopeManager
    {
        private readonly Stack<CacheContext> _scopes;

        public CacheScopeManager()
        {
            _scopes = new Stack<CacheContext>();
        }

        public void EnterScope(CacheContext context)
        {
            _scopes.Push(context);
        }

        public void ExitScope()
        {
            var childScope = _scopes.Pop();

            if (_scopes.Count > 0)
            {
                MergeCacheContexts(_scopes.Peek(), childScope);
            }
        }

        public void AddDependencies(params string[] dependencies)
        {
            if (_scopes.Count > 0)
            {
                _scopes.Peek().AddTag(dependencies);
            }
        }

        public void AddContexts(params string[] contexts)
        {
            if (_scopes.Count > 0)
            {
                _scopes.Peek().AddContext(contexts);
            }
        }

        public void WithExpiryOn(DateTimeOffset expiryOn)
        {
            if (_scopes.Count <= 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            var value = GetMostRestrictiveDateTimeOffset(scope.ExpiresOn, expiryOn);

            if (value.HasValue)
            {
                scope.WithExpiryOn(value.Value);
            }
        }

        public void WithExpiryAfter(TimeSpan expiryAfter)
        {
            if (_scopes.Count <= 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            var value = GetMostRestrictiveTimespan(scope.ExpiresAfter, expiryAfter);

            if (value.HasValue)
            {
                scope.WithExpiryAfter(value.Value);
            }
        }

        public void WithExpirySliding(TimeSpan expirySliding)
        {
            if (_scopes.Count <= 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            var value = GetMostRestrictiveTimespan(scope.ExpiresSliding, expirySliding);

            if (value.HasValue)
            {
                scope.WithExpirySliding(value.Value);
            }
        }

        private static void MergeCacheContexts(CacheContext into, CacheContext from)
        {
            into.AddContext(from.Contexts.ToArray());
            into.AddTag(from.Tags.ToArray());

            var offset = GetMostRestrictiveDateTimeOffset(into.ExpiresOn, from.ExpiresOn);
            if (offset.HasValue)
            {
                into.WithExpiryOn(offset.Value);
            }

            var slidingExpiration = GetMostRestrictiveTimespan(into.ExpiresSliding, from.ExpiresSliding);
            if (slidingExpiration.HasValue)
            {
                into.WithExpirySliding(slidingExpiration.Value);
            }

            var duration = GetMostRestrictiveTimespan(into.ExpiresAfter, from.ExpiresAfter);
            if (duration.HasValue)
            {
                into.WithExpiryAfter(duration.Value);
            }
        }

        private static DateTimeOffset? GetMostRestrictiveDateTimeOffset(DateTimeOffset? a, DateTimeOffset? b)
        {
            if (a.HasValue && b.HasValue)
            {
                return b < a ? b : a;
            }

            return a ?? b;
        }

        private static TimeSpan? GetMostRestrictiveTimespan(TimeSpan? a, TimeSpan? b)
        {
            if (a.HasValue && b.HasValue)
            {
                return b < a ? b : a;
            }

            return a ?? b;
        }
    }
}
