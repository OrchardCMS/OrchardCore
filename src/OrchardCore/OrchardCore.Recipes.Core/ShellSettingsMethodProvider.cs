using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes
{
    public class ShellSettingsMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _tenantName;

        public ShellSettingsMethodProvider(ShellSettings settings)
        {
            _tenantName = new GlobalMethod
            {
                Name = "tenantname",
                Method = serviceProvider => (Func<string>)(() =>
                {
                    return settings.Name;
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _tenantName;
        }
    }
}
