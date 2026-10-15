using Microsoft.Extensions.Primitives;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Workflows.Http.Scripting;

namespace OrchardCore.Tests.Workflows;

public class HttpMethodsProviderTests
{
    [Fact]
    public void DeserializeRequestData_QueryString_ReturnsStringsAndStringLists()
    {
        var (engine, scope) = CreateScope(Get("?name=jane&tag=a&tag=b&token=secret"));

        Assert.Equal("string", engine.Evaluate(scope, "return typeof deserializeRequestData().name;"));
        Assert.Equal(true, engine.Evaluate(scope, "return deserializeRequestData().name === 'jane';"));
        Assert.Equal("JANE", engine.Evaluate(scope, "return deserializeRequestData().name.toUpperCase();"));

        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return deserializeRequestData().tag.length;")));
        Assert.Equal("a|b", engine.Evaluate(scope, "return deserializeRequestData().tag.join('|');"));

        Assert.Equal("""{"name":"jane","tag":["a","b"]}""", engine.Evaluate(scope, "return JSON.stringify(deserializeRequestData());"));
    }

    [Fact]
    public void DeserializeRequestData_Form_ReturnsStringsAndStringLists()
    {
        var (engine, scope) = CreateScope(PostForm(new Dictionary<string, StringValues>
        {
            ["name"] = "jane",
            ["tag"] = new StringValues(["a", "b"]),
        }));

        Assert.Equal("string", engine.Evaluate(scope, "return typeof deserializeRequestData().name;"));
        Assert.Equal(true, engine.Evaluate(scope, "return deserializeRequestData().name === 'jane';"));
        Assert.Equal("JANE", engine.Evaluate(scope, "return deserializeRequestData().name.toUpperCase();"));

        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return deserializeRequestData().tag.length;")));
        Assert.Equal("a|b", engine.Evaluate(scope, "return deserializeRequestData().tag.join('|');"));
    }

    [Fact]
    public void DeserializeRequestData_ReturnsTheSameValuesAsQueryString()
    {
        var (engine, scope) = CreateScope(Get("?name=jane&tag=a&tag=b"));

        Assert.Equal(true, engine.Evaluate(scope, "return deserializeRequestData().name === queryString('name');"));
        Assert.Equal(true, engine.Evaluate(scope, "return JSON.stringify(deserializeRequestData().tag) === JSON.stringify(queryString('tag'));"));
    }

    [Fact]
    public void HttpContextQuery_CanBeComparedAndConcatenated()
    {
        // Request.Query[...] is read straight off the ASP.NET Core API, so the value reaches the script as
        // a StringValues and goes through the wrapping the JavaScript engine registers for that type. That
        // still produces an object rather than a string - queryString() is the way to get a string - but it
        // has to be one a script can compare and concatenate rather than one that throws when it is.
        var (engine, scope) = CreateScope(Get("?name=jane&tag=a&tag=b"));

        Assert.Equal(true, engine.Evaluate(scope, "return httpContext().Request.Query['name'] == 'jane';"));
        Assert.Equal("hello jane", engine.Evaluate(scope, "return 'hello ' + httpContext().Request.Query['name'];"));
        Assert.Equal(4, Convert.ToInt32(engine.Evaluate(scope, "return httpContext().Request.Query['name'].length;")));
        Assert.Equal("JANE", engine.Evaluate(scope, "return httpContext().Request.Query['name'].ToUpper();"));

        Assert.Equal(2, Convert.ToInt32(engine.Evaluate(scope, "return httpContext().Request.Query['tag'].length;")));
        Assert.Equal("a|b", engine.Evaluate(scope, "return httpContext().Request.Query['tag'].join('|');"));
    }

    private static DefaultHttpContext Get(string queryString)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.QueryString = new QueryString(queryString);

        return httpContext;
    }

    private static DefaultHttpContext PostForm(Dictionary<string, StringValues> fields)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Request.Form = new FormCollection(fields);

        return httpContext;
    }

    private static (IScriptingEngine Engine, IScriptingScope Scope) CreateScope(HttpContext httpContext)
    {
        var provider = new HttpMethodsProvider(new HttpContextAccessor { HttpContext = httpContext });

        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .AddSingleton<IGlobalMethodProvider>(provider)
            .BuildServiceProvider();

        var engine = serviceProvider.GetServices<IScriptingEngine>().First(engine => engine.Prefix == "js");

        return (engine, engine.CreateScope(provider.GetMethods(), serviceProvider, null, null));
    }
}
