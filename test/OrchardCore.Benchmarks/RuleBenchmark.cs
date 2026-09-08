using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.Layers.Services;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Modules.OrchardCore.Rules;

namespace OrchardCore.Benchmarks;

[MemoryDiagnoser]
public class RuleBenchmark
{
    private static readonly IScriptingEngine s_engine;
    private static readonly IScriptingScope s_scope;
    private static readonly IRuleService s_ruleService;
    private static readonly Rule s_rule;

    static RuleBenchmark()
    {
        var services = RuleTests.CreateRuleServiceCollection()
            .AddRuleCondition<HomepageCondition, HomepageConditionEvaluator>()
            .AddSingleton<IGlobalMethodProvider, DefaultLayersMethodProvider>()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString("/");
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        services.AddSingleton<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        var serviceProvider = services.BuildServiceProvider();

        var scriptingManager = serviceProvider.GetRequiredService<IScriptingManager>();

        s_engine = scriptingManager.GetScriptingEngine("js");
        s_scope = s_engine.CreateScope(scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), serviceProvider, null, null);

        s_ruleService = serviceProvider.GetRequiredService<IRuleService>();
        s_rule = new Rule
        {
            Conditions =
            [
                new HomepageCondition
                {
                    Value = true,
                }
            ],
        };
    }

    [Benchmark(Baseline = true)]
#pragma warning disable CA1822 // Mark members as static
    public void EvaluateIsHomepageWithJavascript() => s_engine.Evaluate(s_scope, "isHomepage()");

    [Benchmark]
    public async Task EvaluateIsHomepageWithRule() => await s_ruleService.EvaluateAsync(s_rule);
#pragma warning restore CA1822 // Mark members as static
}
