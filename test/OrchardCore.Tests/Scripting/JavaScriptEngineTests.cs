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

    // A prototype chain is built by the script, so its depth is an input. Up to Jint 4.16.2 a property read,
    // a write, an 'in' test or a name resolved inside 'with' walked it one native frame per link, and a long
    // enough chain overflowed the native stack. Constraints.StackOverflowGuard did not cover it, because no
    // script function is entered along the way, so the process died exactly as for unbounded recursion.
    // The chain is now walked in a loop, so an ordinary one of any depth simply resolves. It is built with
    // Object.create() because a '__proto__' literal checks the whole chain for a cycle at every link, which
    // would make building a chain this long take seconds.
    private const string DeepPrototypeChain = "var x = { found: 42 }; for (var i = 0; i < 100000; i++) { x = Object.create(x); }";

    [Theory]
    [InlineData("return x.missing === undefined;")]
    [InlineData("return x.found === 42;")]
    [InlineData("return !('missing' in x);")]
    [InlineData("x.missing = 1; return x.missing === 1;")]
    [InlineData("with (x) { return found === 42; }")]
    public void Evaluate_DeepPrototypeChain_ResolvesInsteadOfKillingTheProcess(string script)
    {
        var (engine, scope) = CreateScope();

        Assert.True((bool)engine.Evaluate(scope, DeepPrototypeChain + script));
    }

    // How deep a chain has to be before the guard steps in depends on the size of the thread's stack, which is
    // larger on Linux than on Windows, so a chain of a fixed length may raise the error on one and simply complete
    // on the other. Either is fine. What these tests pin is that the process survives and the engine still works:
    // on Jint 4.16.x each of these shapes ended the process.
    [Fact]
    public void Evaluate_DeepChainOfProxies_DoesNotKillTheProcess()
    {
        var (engine, scope) = CreateScope();

        // A proxy cannot be walked in a loop, because each one may answer differently. Up to Jint 4.16.2 the
        // forward from one proxy without a trap to the next did not measure the native stack, so this ended
        // the process even with the guard on. It now measures it, and raises a RangeError if the stack runs low.
        var result = engine.Evaluate(scope, """
            var p = {};
            for (var i = 0; i < 100000; i++) { p = new Proxy(p, {}); }
            try { p.missing; return 'completed'; } catch (e) { return e.constructor.name; }
            """);

        string[] outcomes = ["completed", "RangeError"];
        Assert.Contains(Assert.IsType<string>(result), outcomes);
        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return 1 + 1;")));
    }

    // Up to Jint 4.16.2, asking whether a function made by a long '.bind()' chain is a constructor recursed
    // through every link before any guarded call ran, so 'new' or 'class extends' on it ended the process.
    // That question is now answered in a loop. Constructing through the chain still descends it, and that
    // descent is guarded, so it either completes or raises a RangeError; 'class extends' stops earlier, with
    // the TypeError for a superclass without a 'prototype', which a bound function never has.
    private const string DeepBindChain = "var f = function () { }; for (var i = 0; i < 50000; i++) { f = f.bind(null); }";

    [Theory]
    [InlineData("new f();", "constructed", "RangeError")]
    [InlineData("Reflect.construct(f, []);", "constructed", "RangeError")]
    [InlineData("class C extends f { } new C();", "TypeError")]
    public void Evaluate_DeepBindChain_DoesNotKillTheProcess(string construct, params string[] outcomes)
    {
        var (engine, scope) = CreateScope();

        var result = engine.Evaluate(scope, $$"""
            {{DeepBindChain}}
            try { {{construct}} return 'constructed'; } catch (e) { return e.constructor.name; }
            """);

        Assert.Contains(Assert.IsType<string>(result), outcomes);
        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return 1 + 1;")));
    }

    // Up to Jint 4.16.4 these recursions never reached the native stack check the guard relies on, so each
    // ended the process: 'eval' parsed and ran its argument without measuring the stack, also when it is the
    // Symbol.hasInstance method 'instanceof' calls, and a failure leaving a ShadowRealm was copied into a
    // TypeError from inside the catch handling it, one nested exception dispatch per wrapped function. The
    // 'eval' rows and the last row never end, so they always raise the error; the chain of wrapped functions
    // has a fixed length, so it may also complete. The last row needs no chain at all: wrapping a function
    // reads its 'name', and that getter wraps it again.
    [Theory]
    [InlineData("var s = 'eval(s)'; eval(s);", "RangeError")]
    [InlineData("var o = {}; Object.defineProperty(o, Symbol.hasInstance, { value: eval }); var s = 's instanceof o'; s instanceof o;", "RangeError")]
    [InlineData("const sr = new ShadowRealm(); const id = sr.evaluate('x => x'); let f = function () { return 1; }; for (let i = 0; i < 5000; i++) f = id(f); f();", "completed", "TypeError")]
    [InlineData("const sr = new ShadowRealm(); const g = function () {}; Object.defineProperty(g, 'name', { get: sr.evaluate('(function () {})') }); sr.evaluate('f => f')(g);", "TypeError")]
    public void Evaluate_EvalOrShadowRealmRecursion_DoesNotKillTheProcess(string script, params string[] outcomes)
    {
        var (engine, scope) = CreateScope();

        var result = engine.Evaluate(scope, $$"""
            try { {{script}} return 'completed'; } catch (e) { return e.constructor.name; }
            """);

        Assert.Contains(Assert.IsType<string>(result), outcomes);
        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return 1 + 1;")));
    }

    [Fact]
    public void Evaluate_ConcatWithAListFromAMethod_SpreadsItsItems()
    {
        // A .NET array or list returned by a method reaches the script as an array, and up to Jint 4.16.4
        // 'concat' added it as a single element instead of spreading its items.
        var (engine, scope) = CreateScope(
            Method("names", () => new List<string> { "b", "c" }),
            Method("numbers", () => new[] { 2, 3 }));

        Assert.Equal("3: a,b,c", engine.Evaluate(scope, "var r = ['a'].concat(names()); return r.length + ': ' + r.join(',');"));
        Assert.Equal("3: 1,2,3", engine.Evaluate(scope, "var r = [1].concat(numbers()); return r.length + ': ' + r.join(',');"));
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
