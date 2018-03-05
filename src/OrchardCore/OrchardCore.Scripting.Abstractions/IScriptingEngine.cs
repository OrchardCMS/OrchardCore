using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Scripting
{
    public interface IScriptingEngine
    {
        LocalizedString Name { get; }
        string Prefix { get; }
        object Evaluate(IScriptingScope scope, string script);
        IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath);
    }
}
