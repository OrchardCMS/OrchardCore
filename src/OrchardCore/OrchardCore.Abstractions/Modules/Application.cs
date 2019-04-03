using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Modules
{
    public class Application
    {
        private readonly Dictionary<string, Module> _modulesByName;
        private readonly List<Module> _modules;

        public static readonly string ModulesPath = "Areas";
        public static readonly string ModuleName = "Application";
        public static readonly string ModulesRoot = ModulesPath + "/";

        public Application(IHostingEnvironment environment, IEnumerable<Module> modules)
        {
            Name = environment.ApplicationName;
            Path = environment.ContentRootPath;
            Root = Path + '/';
            ModulePath = ModulesRoot + Name;
            ModuleRoot = ModulePath + '/';

            Assembly = Assembly.Load(new AssemblyName(Name));

            _modules = new List<Module>(modules);
            _modulesByName = _modules.ToDictionary(m => m.Name, m => m);
        }

        public string Name { get; }
        public string Path { get; }
        public string Root { get; }
        public string ModulePath { get; }
        public string ModuleRoot { get; }
        public Assembly Assembly { get; }
        public IEnumerable<Module> Modules => _modules;

        public Module GetModule(string name)
        {
            if (!_modulesByName.TryGetValue(name, out var module))
            {
                return new Module(string.Empty);
            }

            return module;
        }
    }
}