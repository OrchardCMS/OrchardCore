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

        public ModuleNamesProvider()
        {
            var assembly = Assembly.Load(new AssemblyName(typeof(Program).Assembly.GetName().Name));

            _moduleNames = assembly
                .GetCustomAttributes<ModuleNameAttribute>()
                .Select(m => m.Name);
        }

        public IEnumerable<string> GetModuleNames() => _moduleNames;
    }
}
