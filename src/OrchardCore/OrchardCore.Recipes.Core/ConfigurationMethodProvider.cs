using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell;
using OrchardCore.Scripting;


namespace OrchardCore.Recipes
{
    public class ConfigurationMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public ConfigurationMethodProvider(ShellSettings settings)
        {
            _globalMethod = new GlobalMethod
            {
                Name = "configuration",
                Method = serviceprovider => (Func<string, object>)(name =>
                {
                    return settings.ShellConfiguration[name];
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
