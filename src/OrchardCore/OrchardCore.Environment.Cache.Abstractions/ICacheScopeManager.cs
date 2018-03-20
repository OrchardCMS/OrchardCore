namespace OrchardCore.Environment.Cache
{
    public interface ICacheScopeManager
    {
        void EnterScope(CacheContext context);
        void ExitScope();
        /// <summary> 
        /// Add the given dependencies to the current innermost cache context
        /// </summary>
        /// <param name="dependencies">The dependencies to add</param>
        void AddDependencies(params string[] dependencies);
    }
}