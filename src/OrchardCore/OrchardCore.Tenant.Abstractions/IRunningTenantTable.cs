namespace OrchardCore.Tenant
{
    public interface IRunningTenantTable
    {
        void Add(TenantSettings settings);
        void Remove(TenantSettings settings);
        TenantSettings Match(string host, string appRelativeCurrentExecutionFilePath);
    }
}