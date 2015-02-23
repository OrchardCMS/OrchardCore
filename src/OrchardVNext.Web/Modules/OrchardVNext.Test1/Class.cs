using System;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Test1 {
    public interface ITestDependency : IDependency {
        string SayHi(string line);
    }

    public class Class : ITestDependency {
        private readonly ShellSettings _shellSettings;
        public Class(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public string SayHi(string line) {
            return string.Format("Hi from tenant {0} - {1}", _shellSettings.Name, line);
        }
    }
}