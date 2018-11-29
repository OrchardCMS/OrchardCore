using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting
{
    public class DefaultScriptingManager : IScriptingManager
    {
        private readonly IEnumerable<IScriptingEngine> _engines;
        private readonly IServiceProvider _serviceProvider;

        public DefaultScriptingManager(
            IEnumerable<IScriptingEngine> engines,
            IEnumerable<IGlobalMethodProvider> globalMethodProviders,
            IServiceProvider serviceProvider)
        {
            _engines = engines;
            GlobalMethodProviders = new List<IGlobalMethodProvider>(globalMethodProviders);
            _serviceProvider = serviceProvider;
        }

        public IList<IGlobalMethodProvider> GlobalMethodProviders { get; }

        public object Evaluate(string directive,
            IFileProvider fileProvider,
            string basePath,
            IEnumerable<IGlobalMethodProvider> scopedMethodProviders)
        {
            var directiveIndex = directive.IndexOf(":");

            if (directiveIndex == -1 || directiveIndex >= directive.Length - 2)
            {
                return directive;
            }

            var prefix = directive.Substring(0, directiveIndex);
            var script = directive.Substring(directiveIndex + 1);

            var engine = GetScriptingEngine(prefix);
            if (engine == null)
            {
                return directive;
            }

            var methodProviders = scopedMethodProviders != null ? GlobalMethodProviders.Concat(scopedMethodProviders) : GlobalMethodProviders;
            var scope = engine.CreateScope(methodProviders.SelectMany(x => x.GetMethods()), _serviceProvider, fileProvider, basePath);
            return engine.Evaluate(scope, script);
        }

        public IScriptingEngine GetScriptingEngine(string prefix)
        {
            return _engines.FirstOrDefault(x => x.Prefix == prefix);
        }
    }
}
