using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace Orchard.Scripting
{
    public interface IScriptingEngine
    {
        LocalizedString Name { get; }
        string Prefix { get; }
        object Evaluate(IScriptingScope scope, string script);
        IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider);
    }
}
