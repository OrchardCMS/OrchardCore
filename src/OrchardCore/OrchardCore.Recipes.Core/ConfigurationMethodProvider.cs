using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
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
                Method = serviceprovider => (Func<string, object, object>)((key, defaultValue) => configuration.GetValue<object>(key, defaultValue)),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
