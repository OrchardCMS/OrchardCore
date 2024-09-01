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

namespace OrchardCore.Benchmark;

[MemoryDiagnoser]
public class RuleBenchmark
{
    private static readonly IScriptingEngine _engine;
    private static readonly IScriptingScope _scope;
    private static readonly IRuleService _ruleService;
    private static readonly Rule _rule;

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

        _engine = scriptingManager.GetScriptingEngine("js");
        _scope = _engine.CreateScope(scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), serviceProvider, null, null);

        _ruleService = serviceProvider.GetRequiredService<IRuleService>();
        _rule = new Rule
        {
            Conditions =
            [
                new HomepageCondition
                {
                    Value = true
                }
            ]
        };
    }

    [Benchmark(Baseline = true)]
#pragma warning disable CA1822 // Mark members as static
    public void EvaluateIsHomepageWithJavascript() => _engine.Evaluate(_scope, "isHomepage()");

    [Benchmark]
    public async Task EvaluateIsHomepageWithRule() => await _ruleService.EvaluateAsync(_rule);
#pragma warning restore CA1822 // Mark members as static
}
