using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Modules
{
    public class Application
    {
        private readonly Dictionary<string, Module> _modulesByName;
        private readonly List<Module> _modules;

        public const string ModulesPath = "Areas";
        public const string ModulesRoot = ModulesPath + "/";

        public const string ModuleName = "Application Main Feature";
        public const string ModuleDescription = "Provides components defined at the application level.";
        public static readonly string ModulePriority = Int32.MinValue.ToString();
        public const string ModuleCategory = "Application";

        public const string DefaultFeatureId = "Application.Default";
        public const string DefaultFeatureName = "Application Default Feature";
        public const string DefaultFeatureDescription = "Adds a default feature to the application's module.";

        public Application(IHostEnvironment environment, IEnumerable<Module> modules)
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
                return new Module(String.Empty);
            }

            return module;
        }
    }
}
