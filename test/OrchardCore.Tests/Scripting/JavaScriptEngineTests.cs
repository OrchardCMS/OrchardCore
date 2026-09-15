using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using Jint.Runtime;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Scripting;

public class JavaScriptEngineTests
{
    [Fact]
    public void Evaluate_WhenTheScriptTextIsAlreadyUsedAsACacheKey_StillEvaluatesTheScript()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var serviceProvider = services.BuildServiceProvider();

        const string script = "return 1 + 1;";

        // Another component of the application happens to use the same string as a cache key in the
        // shared memory cache. Prepared scripts have to be namespaced so that they cannot collide.
        serviceProvider.GetRequiredService<IMemoryCache>().Set(script, "an unrelated value");

        var engine = serviceProvider.GetServices<IScriptingEngine>().First(engine => engine.Prefix == "js");
        var scope = engine.CreateScope([], serviceProvider, null, null);

        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, script)));
    }

    [Fact]
    public void Evaluate_DynamicJsonValues_AreReadThroughTheNodeTheyCarry()
    {
        // The dynamic JSON types reach a script the way content does: through a member declared as dynamic,
        // so the value arrives under an exposed type of object and the wrap handler substitutes the node it
        // carries. What a script sees has to be that node, whatever the member said it was.
        var (engine, scope) = CreateScope(
            Method("dynamicObject", () => new JsonDynamicObject(JObject.Parse("""{"name":"jane","age":33}"""))),
            Method("dynamicArray", () => new JsonDynamicArray(JsonNode.Parse("""["a","b","c"]""").AsArray())),
            Method("dynamicValue", () => new JsonDynamicValue(JsonValue.Create(42))));

        Assert.Equal("jane", engine.Evaluate(scope, "return dynamicObject().name;"));
        Assert.Equal(33, Convert.ToInt32(engine.Evaluate(scope, "return dynamicObject().age;")));
        Assert.Equal("""{"name":"jane","age":33}""", engine.Evaluate(scope, "return JSON.stringify(dynamicObject());"));

        Assert.Equal("b", engine.Evaluate(scope, "return dynamicArray()[1];"));
        Assert.Equal(3, Convert.ToInt32(engine.Evaluate(scope, "return dynamicArray().length;")));
        Assert.Equal("0+1+2", engine.Evaluate(scope, "return Object.keys(dynamicArray()).join('+');"));
        Assert.Equal("""["a","b","c"]""", engine.Evaluate(scope, "return JSON.stringify(dynamicArray());"));

        Assert.Equal("42", engine.Evaluate(scope, "return String(dynamicValue());"));
    }

    [Theory]
    // A plain call, and then the routes that reach a function body without one. The engine's older
    // recursion lanes are probed at the call expression, so only the first of these was ever covered;
    // the stack probe measures the stack itself and sees all four.
    [InlineData("function f() { return 1 + f(); } f();")]
    [InlineData("var o = { get boom() { return o.boom + 1; } }; o.boom;")]
    [InlineData("function C() { new C(); } new C();")]
    [InlineData("var o = { valueOf: function () { return o + 1; } }; o + 1;")]
    public void Evaluate_UnboundedRecursion_RaisesAnErrorInsteadOfKillingTheProcess(string script)
    {
        var (engine, scope) = CreateScope();

        // Without Constraints.StackOverflowGuard this does not throw: the process is killed by a native
        // stack overflow, so there is nothing for a test to assert and the whole run disappears. That is
        // exactly what makes it worth pinning - the failure mode is the absence of a failure.
        var exception = Assert.Throws<JavaScriptException>(() => engine.Evaluate(scope, script));

        Assert.Contains("Maximum call stack size exceeded", exception.Message);
    }

    [Fact]
    public void Evaluate_AfterUnboundedRecursion_TheEngineIsStillUsable()
    {
        var (engine, scope) = CreateScope();

        Assert.Throws<JavaScriptException>(() => engine.Evaluate(scope, "function f() { return 1 + f(); } f();"));

        // The point of turning a process kill into an error value: the request that ran the script is the
        // only thing that fails, and the scope it failed in still works.
        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return 1 + 1;")));
    }

    [Fact]
    public void Evaluate_UnboundedRecursion_IsCatchableByTheScriptItself()
    {
        var (engine, scope) = CreateScope();

        // A RangeError, not a host-only exception, so a script that wants to recurse to its own limit can.
        Assert.Equal("RangeError", engine.Evaluate(scope, """
            function f() { return 1 + f(); }
            try { f(); return 'no error'; } catch (e) { return e.constructor.name; }
            """));
    }

    private static GlobalMethod Method(string name, Func<dynamic> value)
        => new()
        {
            Name = name,
            Method = _ => value,
        };

    private static (IScriptingEngine Engine, IScriptingScope Scope) CreateScope(params GlobalMethod[] methods)
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .BuildServiceProvider();

        var engine = serviceProvider.GetServices<IScriptingEngine>().First(engine => engine.Prefix == "js");

        return (engine, engine.CreateScope(methods, serviceProvider, null, null));
    }
}
