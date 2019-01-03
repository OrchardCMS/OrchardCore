using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Modules
{
    public interface IApplicationContext
    {
        Application Application { get; }
    }

    public class ModularApplicationContext : IApplicationContext
    {
        private readonly IHostingEnvironment _environment;
        private readonly IEnumerable<IModuleNamesProvider> _moduleNamesProviders;
        private Application _application;
        private static readonly object _initLock = new object();

        public ModularApplicationContext(IHostingEnvironment environment, IEnumerable<IModuleNamesProvider> moduleNamesProviders)
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
                    if (_application == null)
                    {
                        _application = new Application(_environment, GetModules());
                    }
                }
            }
        }

        private IEnumerable<Module> GetModules()
        {
            var modules = new ConcurrentBag<Module>();
            modules.Add(new Module(_environment.ApplicationName, true));

            foreach (var provider in _moduleNamesProviders)
            {
                var names = provider.GetModuleNames().Distinct();

                Parallel.ForEach(names, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (name) =>
                {
                    modules.Add(new Module(name, name == _environment.ApplicationName));
                });
            }

            return modules;
        }
    }
}