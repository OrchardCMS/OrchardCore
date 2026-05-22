using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope
{
    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
    {
        Engine = engine;
        ServiceProvider = serviceProvider;
        
        foreach (var method in methods)
        {
            if (method.Method != null)
            {
                Engine.SetValue(method.Name, method.Method(ServiceProvider));
            }
            
            if (method.AsyncMethod != null)
            {
                Engine.SetValue(method.Name + "Async", method.AsyncMethod(ServiceProvider));
            }
        }
    }

    public Engine Engine { get; }

    public IServiceProvider ServiceProvider { get; }
}
