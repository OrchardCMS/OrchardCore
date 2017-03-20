using OrchardCore.Tenant;

namespace Orchard.Demo.Services
{
    public interface ITestDependency
    {
        string SayHi(string line);
    }

    public class ClassFoo : ITestDependency
    {
        private readonly TenantSettings _tenantSettings;
        public ClassFoo(TenantSettings tenantSettings)
        {
            _tenantSettings = tenantSettings;
        }

        public string SayHi(string line)
        {
            return string.Format("Hi from tenant {0} - {1}", _tenantSettings.Name, line);
        }
    }
}