namespace OrchardCore.Environment.Cache
{
    public interface ICacheScopeManager
    {
        void EnterScope(CacheContext context);
        void ExitScope();
        /// <summary>
        /// Add the given tag(s) to the innermost cache context
        /// </summary>
        /// <param name="tag">The tag(s) to add</param>
        void AddTag(params string[] tag);
    }
}