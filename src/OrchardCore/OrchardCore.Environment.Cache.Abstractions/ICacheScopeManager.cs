namespace OrchardCore.Environment.Cache
{
    public interface ICacheScopeManager
    {
        void EnterScope(CacheContext context);
        void ExitScope();
    }
}