using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.Testing
{
    public class ModuleNamesProvider : IModuleNamesProvider
    {
        private readonly IEnumerable<string> _moduleNames;

        public ModuleNamesProvider(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _moduleNames = Assembly.Load(new AssemblyName(assembly.GetName().Name))
                .GetCustomAttributes<ModuleNameAttribute>()
                .Select(m => m.Name);
        }

        public IEnumerable<string> GetModuleNames() => _moduleNames;
    }
}
