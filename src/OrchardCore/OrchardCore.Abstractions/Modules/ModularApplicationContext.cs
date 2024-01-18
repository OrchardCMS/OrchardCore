using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Modules
{
    public class ModularApplicationContext : IApplicationContext
    {
        private readonly IHostEnvironment _environment;
        private readonly IEnumerable<IModuleNamesProvider> _moduleNamesProviders;
        private Application _application;
        private static readonly object _initLock = new();

        public ModularApplicationContext(IHostEnvironment environment, IEnumerable<IModuleNamesProvider> moduleNamesProviders)
        {
            _environment = environment;
            _moduleNamesProviders = moduleNamesProviders;
        }

        public Application Application
        {
            get
            {
                EnsureInitialized();
                return _application;
            }
        }

        private void EnsureInitialized()
        {
            if (_application == null)
            {
                lock (_initLock)
                {
                    _application ??= new Application(_environment, GetModules());
                }
            }
        }

        private IEnumerable<Module> GetModules()
        {
            var modules = new ConcurrentBag<Module>
            {
                new Module(_environment.ApplicationName, true),
            };

            var names = _moduleNamesProviders
                .SelectMany(p => p.GetModuleNames())
                .Where(n => n != _environment.ApplicationName)
                .Distinct();

            Parallel.ForEach(names, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (name) =>
            {
                modules.Add(new Module(name, false));
            });

            return modules;
        }
    }
}
