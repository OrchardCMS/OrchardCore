using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Scripting
{
    public class DefaultScriptingManager : IScriptingManager
    {
        private readonly IEnumerable<IScriptingEngine> _engines;

        public DefaultScriptingManager(
            IEnumerable<IScriptingEngine> engines,
            IEnumerable<IGlobalMethodProvider> globalMethodProviders)
        {
            _engines = engines;
            GlobalMethodProviders = new List<IGlobalMethodProvider>(globalMethodProviders);
        }

        public IList<IGlobalMethodProvider> GlobalMethodProviders { get; }

        public Task<object> EvaluateAsync(string directive,
            IFileProvider fileProvider,
            string basePath,
            IEnumerable<IGlobalMethodProvider> scopedMethodProviders)
        {
            var directiveIndex = directive.IndexOf(":");

            if (directiveIndex == -1 || directiveIndex >= directive.Length - 2)
            {
                return Task.FromResult((object)directive);
            }

            var prefix = directive.Substring(0, directiveIndex);
            var script = directive.Substring(directiveIndex + 1);

            var engine = GetScriptingEngine(prefix);
            if (engine == null)
            {
                return Task.FromResult((object)directive);
            }

            var methodProviders = scopedMethodProviders != null ? GlobalMethodProviders.Concat(scopedMethodProviders) : GlobalMethodProviders;
            var scope = engine.CreateScope(methodProviders.SelectMany(x => x.GetMethods()), ShellScope.Services, fileProvider, basePath);
            return engine.EvaluateAsync(scope, script);
        }

        public IScriptingEngine GetScriptingEngine(string prefix)
        {
            return _engines.FirstOrDefault(x => x.Prefix == prefix);
        }
    }
}
