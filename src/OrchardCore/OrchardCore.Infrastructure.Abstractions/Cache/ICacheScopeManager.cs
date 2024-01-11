using System;

namespace OrchardCore.Environment.Cache
{
    public interface ICacheScopeManager
    {
        void EnterScope(CacheContext context);
        void ExitScope();
        /// <summary>
        /// Adds the given dependencies to the current innermost cache context
        /// </summary>
        /// <param name="dependencies">The dependencies to add</param>
        void AddDependencies(params string[] dependencies);
        /// <summary>
        /// Adds the given contexts to the current innermost cache context
        /// </summary>
        /// <param name="contexts">The contexts to add</param>
        void AddContexts(params string[] contexts);
        void WithExpiryOn(DateTimeOffset expiryOn);
        void WithExpiryAfter(TimeSpan expiryAfter);
        void WithExpirySliding(TimeSpan expirySliding);
    }
}
