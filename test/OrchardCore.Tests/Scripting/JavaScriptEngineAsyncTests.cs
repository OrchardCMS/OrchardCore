using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Scripting;

/// <summary>
/// Asynchronous scripts, run the way a workflow script or a layer rule runs them: through
/// <see cref="IScriptingEngine.EvaluateAsync"/>, with the globals a method provider contributes, including the
/// <c>xAsync</c> variant of a method that has one.
/// </summary>
/// <remarks>
/// Up to Jint 4.16.2, reading an element or a computed member straight off an awaited value -
/// <c>(await listItemsAsync())[0]</c> - ran the read against the suspended <c>await</c> rather than its result.
/// The error that raised was swallowed, but it made the async function resume from its first statement, so
/// every statement before that point ran a second time. An awaited call was not repeated - the engine replays
/// its settled result - but anything the function did without awaiting it was, such as a synchronous global.
/// </remarks>
public class JavaScriptEngineAsyncTests
{
    [Theory]
    [InlineData("(await listItemsAsync())[0]")]
    [InlineData("(await listItemsAsync())[index]")]
    public async Task EvaluateAsync_AMemberReadOfAnAwaitedValue_DoesNotRepeatTheStatementsBeforeIt(string read)
    {
        var (engine, scope, provider) = CreateScope();

        var result = await engine.EvaluateAsync(scope, $$"""
            return (async function () {
                record('before');
                createItem('draft');
                const index = 0;
                const item = {{read}};
                record('after');
                return item;
            })();
            """, TestContext.Current.CancellationToken);

        // A repeated createItem() is a second content item on a real site.
        Assert.Equal(["draft"], provider.Created);
        Assert.Equal(["before", "after"], provider.Recorded);
        Assert.Equal("draft", result);
    }

    [Fact]
    public async Task EvaluateAsync_AMemberReadOfAnAwaitedValue_DoesNotRepeatAnAsyncCallThatWasNotAwaited()
    {
        var (engine, scope, provider) = CreateScope();

        var result = await engine.EvaluateAsync(scope, """
            return (async function () {
                await createItemAsync('awaited');
                createItemAsync('not awaited');
                return (await listItemsAsync())[1];
            })();
            """, TestContext.Current.CancellationToken);

        Assert.Equal(["awaited", "not awaited"], provider.Created);
        Assert.Equal("not awaited", result);
    }

    private static (IScriptingEngine Engine, IScriptingScope Scope, ItemsMethodProvider Provider) CreateScope()
    {
        var provider = new ItemsMethodProvider();

        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .AddSingleton<IGlobalMethodProvider>(provider)
            .BuildServiceProvider();

        var scriptingManager = serviceProvider.GetRequiredService<IScriptingManager>();
        var engine = scriptingManager.GetScriptingEngine("js");

        // The scope a layer rule builds: every registered method, the asynchronous variants among them.
        var scope = engine.CreateScope(scriptingManager.GlobalMethodProviders.SelectMany(p => p.GetMethods()), serviceProvider, null, null);

        return (engine, scope, provider);
    }

    /// <summary>
    /// Shaped like the content methods: one method with a synchronous and an asynchronous variant, which a
    /// script sees as <c>createItem</c> and <c>createItemAsync</c>.
    /// </summary>
    private sealed class ItemsMethodProvider : IGlobalMethodProvider
    {
        public List<string> Created { get; } = [];

        public List<string> Recorded { get; } = [];

        public IEnumerable<GlobalMethod> GetMethods() =>
        [
            new GlobalMethod
            {
                Name = "createItem",
                Method = _ => (Action<string>)Created.Add,
                AsyncMethod = _ => (Func<string, Task>)(name =>
                {
                    Created.Add(name);
                    return Task.CompletedTask;
                }),
            },
            new GlobalMethod
            {
                Name = "listItems",
                AsyncMethod = _ => (Func<Task<string[]>>)(() => Task.FromResult(Created.ToArray())),
            },
            new GlobalMethod
            {
                Name = "record",
                Method = _ => (Action<string>)Recorded.Add,
            },
        ];
    }
}
