using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting
{
    public interface IScriptingEngine
    {
        string Prefix { get; }
        Task<object> EvaluateAsync(IScriptingScope scope, string script);
        IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath);
    }
}
