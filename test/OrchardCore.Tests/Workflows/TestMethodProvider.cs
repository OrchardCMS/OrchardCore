using System;
using System.Collections.Generic;
using OrchardCore.Scripting;

namespace OrchardCore.Tests.Workflows
{
    public class TestMethodProvider : IGlobalMethodProvider
    {
        public TestMethodProvider()
        {
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[]
            {
                new GlobalMethod { Name = "method1", Method = serviceProvider => (Func<object>)(() => 42) },
                new GlobalMethod { Name = "method2", Method = serviceProvider => (Func<string, object>)(x => x.ToString() + " - tested") }
            };
        }
    }
}
