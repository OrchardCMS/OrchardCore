using OrchardCore.Environment.Shell;

namespace OrchardCore.Demo.Services
{
    public interface ITestDependency
    {
        string SayHi(string line);
    }

    public class ClassFoo : ITestDependency
    {
        private readonly ShellSettings _shellSettings;
        public ClassFoo(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public string SayHi(string line)
        {
            return string.Format("Hi from tenant {0} - {1}", _shellSettings.Name, line);
        }
    }
}
