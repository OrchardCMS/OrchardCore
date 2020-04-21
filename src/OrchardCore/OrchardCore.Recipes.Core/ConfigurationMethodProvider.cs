using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes
{
    public class ConfigurationMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public ConfigurationMethodProvider(IShellConfiguration configuration)
        {
            _globalMethod = new GlobalMethod
            {
                Name = "configuration",
                Method = serviceprovider => (Func<string, object>)(name => configuration[name])
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
