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
    public async Task ShouldEvaluateRuleFalseWhenNoConditions()
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
    public async Task ShouldEvaluateHomepage(string path, bool isHomepage, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new HomepageCondition
                {
                    Value = isHomepage
                }
            ]
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
    public async Task ShouldEvaluateBoolean(bool boolean, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new BooleanCondition { Value = boolean }
            ]
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
    public async Task ShouldEvaluateAny(bool first, bool second, bool expected)
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
                    ]
                }
            ]
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
    public async Task ShouldEvaluateUrlEquals(string path, string requestPath, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new UrlCondition
                {
                    Value = path,
                    Operation = new StringEqualsOperator()
                }
            ]
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
    public async Task ShouldEvaluateJavascriptCondition(string script, string requestPath, bool expected)
    {
        var rule = new Rule
        {
            Conditions =
            [
                new JavascriptCondition
                {
                    Script = script
                }
            ]
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

    public static ServiceCollection CreateRuleServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddOptions<ConditionOptions>();

        services.AddTransient<IConditionResolver, ConditionResolver>();
        services.AddTransient<IConditionOperatorResolver, ConditionOperatorResolver>();

        services.AddTransient<IRuleService, RuleService>();

        services.AddTransient<AllConditionEvaluator>();

        services.AddLocalization();
        services.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
        services.AddTransient<IConfigureOptions<ConditionOperatorOptions>, ConditionOperatorConfigureOptions>();

        return services;
    }
}
