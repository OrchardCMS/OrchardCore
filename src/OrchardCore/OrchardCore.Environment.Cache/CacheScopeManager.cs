using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Cache
{
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

        public void AddDependencies(params string[] tag)
        {
            if (_scopes.Count > 0)
            {
                _scopes.Peek().AddTag(tag);
            }
        }

        private void MergeCacheContexts(CacheContext into, CacheContext from)
        {
            into.AddContext(from.Contexts.ToArray());
            into.AddTag(from.Tags.ToArray());

            var slidingExpiration = GetMostRestrictiveTimespan(into.SlidingExpirationWindow, from.SlidingExpirationWindow);
            if (slidingExpiration.HasValue)
            {
                into.WithSlidingExpiration(slidingExpiration.Value);
            }

            var duration = GetMostRestrictiveTimespan(into.Duration, from.Duration);
            if (duration.HasValue)
            {
                into.WithDuration(duration.Value);
            }
        }

        private TimeSpan? GetMostRestrictiveTimespan(TimeSpan? a, TimeSpan? b)
        {
            if (a.HasValue && b.HasValue)
            {
                return b < a ? b : a;
            }

            return a ?? b;
        }
    }
}