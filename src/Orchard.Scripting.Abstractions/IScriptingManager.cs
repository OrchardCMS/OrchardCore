using System.Collections;
using System.Collections.Generic;

namespace Orchard.Scripting
{
    public interface IScriptingManager
    {
        IScriptingEngine GetScriptingEngine(string prefix);
        object Evaluate(string directive);  
        IList<IGlobalMethodProvider> GlobalMethodProviders { get; }
    }
}
