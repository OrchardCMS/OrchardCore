using Jint.Runtime;
using OrchardCore.Layers.Services;
using OrchardCore.Localization;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Tests.Modules.OrchardCore.Rules;

public class RuleTests
{
    [Fact]
    public async Task Evaluate_NoConditions_Succeeds()
    {
        var rule = new Rule();

        var services = CreateRuleServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();

        Assert.False(await ruleService.EvaluateAsync(rule));
    }

    [Theory]
    [InlineData("/", true, true)]
    [InlineData("/notthehomepage", true, false)]
    [InlineData("/", false, false)]
    [InlineData("/notthehomepage", false, true)]
    public async Task Evaluate_Homepage_Succeeds(string path, bool isHomepage, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new HomepageCondition
                {
                    Value = isHomepage,
                }
            ],
        };

        var services = CreateRuleServiceCollection()
            .AddRuleCondition<HomepageCondition, HomepageConditionEvaluator>();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(path);
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        services.AddSingleton<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();

        Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Evaluate_Boolean_Succeeds(bool boolean, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new BooleanCondition { Value = boolean }
            ],
        };

        var services = CreateRuleServiceCollection()
            .AddRuleCondition<BooleanCondition, BooleanConditionEvaluator>();

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();

        Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    public async Task Evaluate_Any_Succeeds(bool first, bool second, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new AnyConditionGroup
                {
                    Conditions =
                    [
                        new BooleanCondition { Value = first },
                        new BooleanCondition { Value = second }
                    ],
                }
            ],
        };

        var services = CreateRuleServiceCollection()
            .AddRuleCondition<AnyConditionGroup, AnyConditionEvaluator>()
            .AddRuleCondition<BooleanCondition, BooleanConditionEvaluator>();

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();

        Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
    }

    [Theory]
    [InlineData("/foo", "/foo", true)]
    [InlineData("/bar", "/foo", false)]
    public async Task Evaluate_UrlEquals_Succeeds(string path, string requestPath, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new UrlCondition
                {
                    Value = path,
                    Operation = new StringEqualsOperator(),
                }
            ],
        };

        var services = CreateRuleServiceCollection()
            .AddRuleCondition<UrlCondition, UrlConditionEvaluator>();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(requestPath);
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        services.AddSingleton<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();

        Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
    }

    [Theory]
    [InlineData("isHomepage()", "/", true)]
    [InlineData("isHomepage()", "/foo", false)]
    public async Task Evaluate_JavascriptCondition_Succeeds(string script, string requestPath, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new JavascriptCondition
                {
                    Script = script,
                }
            ],
        };

        var services = CreateRuleServiceCollection()
            .AddRuleCondition<JavascriptCondition, JavascriptConditionEvaluator>()
            .AddSingleton<IGlobalMethodProvider, DefaultLayersMethodProvider>()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(requestPath);
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        services.AddSingleton<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();
        Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
    }

    [Fact]
    public async Task Evaluate_JavascriptConditionWithAsyncGlobalMethod_Succeeds()
    {
        var rule = new Rule
        {
            Conditions =
            [
                new JavascriptCondition
                {
                    Script = "isAsyncAllowedAsync()",
                }
            ],
        };

        var services = CreateRuleServiceCollection()
            .AddRuleCondition<JavascriptCondition, JavascriptConditionEvaluator>()
            .AddSingleton<IGlobalMethodProvider, AsyncBooleanMethodProvider>()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var serviceProvider = services.BuildServiceProvider();

        var ruleService = serviceProvider.GetRequiredService<IRuleService>();

        Assert.True(await ruleService.EvaluateAsync(rule));
    }

    [Fact]
    public async Task Evaluate_JavascriptConditionsDeclaringTheSameName_EachSucceed()
    {
        // The rules of two or more layers, written independently and each validated on its own in the editor,
        // are evaluated one after the other by the same services during a request.
        var ruleService = CreateJavascriptRuleService("/");

        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("const isHome = isHomepage(); isHome")));
        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("const isHome = isHomepage(); let other = 1; class Helper {} isHome")));
        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("let other = 2; class Helper {} isHomepage()")));
    }

    [Fact]
    public async Task Evaluate_JavascriptCondition_DoesNotSeeTheGlobalsOfAnEarlierCondition()
    {
        var ruleService = CreateJavascriptRuleService("/");

        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("var seen = true; globalThis.count = 1; function helper() { return true; } helper()")));
        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("typeof seen === 'undefined' && typeof count === 'undefined' && typeof helper === 'undefined'")));
    }

    [Fact]
    public async Task Evaluate_JavascriptConditionThatThrows_DoesNotAffectTheNextCondition()
    {
        var ruleService = CreateJavascriptRuleService("/");

        await Assert.ThrowsAnyAsync<JavaScriptException>(async () => await ruleService.EvaluateAsync(CreateJavascriptRule("const value = 1; var leaked = true; throw new Error('failed');")));

        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("const value = 2; value === 2 && typeof leaked === 'undefined'")));
    }

    [Fact]
    public async Task Evaluate_SeveralJavascriptConditionsWithAsyncGlobalMethod_Succeed()
    {
        var services = CreateRuleServiceCollection()
            .AddRuleCondition<JavascriptCondition, JavascriptConditionEvaluator>()
            .AddSingleton<IGlobalMethodProvider, AsyncBooleanMethodProvider>()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var ruleService = services.BuildServiceProvider().GetRequiredService<IRuleService>();

        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("const allowed = isAsyncAllowedAsync(); allowed")));
        Assert.True(await ruleService.EvaluateAsync(CreateJavascriptRule("const allowed = isAsyncAllowedAsync(); allowed")));
    }

    private static Rule CreateJavascriptRule(string script)
        => new()
        {
            Conditions =
            [
                new JavascriptCondition
                {
                    Script = script,
                }
            ],
        };

    private static IRuleService CreateJavascriptRuleService(string requestPath)
    {
        var services = CreateRuleServiceCollection()
            .AddRuleCondition<JavascriptCondition, JavascriptConditionEvaluator>()
            .AddSingleton<IGlobalMethodProvider, DefaultLayersMethodProvider>()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(requestPath);
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        services.AddSingleton<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        return services.BuildServiceProvider().GetRequiredService<IRuleService>();
    }

    public static ServiceCollection CreateRuleServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddOptions<ConditionOptions>();

        services.AddTransient<IConditionResolver, ConditionResolver>();
        services.AddTransient<IConditionOperatorResolver, ConditionOperatorResolver>();

        services.AddRules();

        services.AddTransient<AllConditionEvaluator>();

        services.AddLocalization();
        services.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
        services.AddTransient<IConfigureOptions<ConditionOperatorOptions>, ConditionOperatorConfigureOptions>();

        return services;
    }

    private sealed class AsyncBooleanMethodProvider : IGlobalMethodProvider
    {
        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return new GlobalMethod
            {
                Name = "isAsyncAllowed",
                Method = serviceProvider => (Func<bool>)(() => false),
                AsyncMethod = serviceProvider => (Func<Task<bool>>)(async () =>
                {
                    await Task.Yield();
                    return true;
                }),
            };
        }
    }
}
