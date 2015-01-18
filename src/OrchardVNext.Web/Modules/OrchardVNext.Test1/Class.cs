using System;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Test1 {
    public interface ITestDependency : IDependency {
        string SayHi();
    }

    public class Class : ITestDependency {
        private readonly ShellSettings _shellSettings;
        public Class(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public string SayHi() {
            return string.Format("Hi from tenant {0}", _shellSettings.Name);
        }
    }
}