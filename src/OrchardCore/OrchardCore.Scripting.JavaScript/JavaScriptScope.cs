using Jint;

namespace OrchardCore.Scripting.JavaScript;

public class JavaScriptScope : IScriptingScope
{
    private readonly GlobalMethod[] _methods;

    public JavaScriptScope(Engine engine, IServiceProvider serviceProvider, IEnumerable<GlobalMethod> methods)
    {
        Engine = engine;
        ServiceProvider = serviceProvider;
        _methods = methods.ToArray();

        SetGlobalMethods(useAsyncMethods: false);
    }

    public Engine Engine { get; }

    public IServiceProvider ServiceProvider { get; }

    public void UseSyncMethods() => SetGlobalMethods(useAsyncMethods: false);

    public void UseAsyncMethods() => SetGlobalMethods(useAsyncMethods: true);

    private void SetGlobalMethods(bool useAsyncMethods)
    {
        foreach (var method in _methods)
        {
            var methodFactory = useAsyncMethods && method.AsyncMethod != null
                ? method.AsyncMethod
                : method.Method;

            Engine.SetValue(method.Name, methodFactory(ServiceProvider));
        }
    }
}
