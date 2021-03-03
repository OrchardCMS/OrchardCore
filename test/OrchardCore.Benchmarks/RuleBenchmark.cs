using System;
using System.Collections.Generic;
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

namespace OrchardCore.Benchmark
{
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
                .AddCondition<HomepageCondition, HomepageConditionEvaluator, ConditionFactory<HomepageCondition>>()
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
                Conditions = new List<Condition>
                {
                    new HomepageCondition 
                    { 
                        Value = true 
                    }
                }
            };
        }

        // Summary 3rd March 2021: dotnet run -c Release --filter RuleBenchmark --framework netcoreapp3.1 --job short

        //BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
        //Intel Core i9-9980HK CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
        //.NET Core SDK = 5.0.200
        //  [Host]   : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT
        //  ShortRun : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT

        //Job=ShortRun IterationCount = 3  LaunchCount=1
        //WarmupCount=3

        //|                           Method |     Mean |    Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
        //|--------------------------------- |---------:|---------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
        //| EvaluateIsHomepageWithJavascript | 3.991 us | 3.824 us | 0.2096 us |  1.00 |    0.00 | 0.1373 | 0.0381 |      - |    1168 B |
        //|       EvaluateIsHomepageWithRule | 1.038 us | 4.620 us | 0.2532 us |  0.26 |    0.05 | 0.0277 | 0.0105 | 0.0010 |     224 B |


        // Summary 3rd March 2021: dotnet run -c Release --filter RuleBenchmark --framework net5.0 --job short

        //BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
        //Intel Core i9-9980HK CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
        //.NET Core SDK = 5.0.200
        //  [Host]   : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT
        //  ShortRun : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT

        //Job=ShortRun IterationCount = 3  LaunchCount=1
        //WarmupCount=3

        //|                           Method |       Mean |      Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
        //|--------------------------------- |-----------:|-----------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
        //| EvaluateIsHomepageWithJavascript | 4,122.9 ns | 6,853.9 ns | 375.69 ns |  1.00 |    0.00 | 0.1221 | 0.0305 |     - |    1040 B |
        //|       EvaluateIsHomepageWithRule |   772.6 ns |   750.9 ns |  41.16 ns |  0.19 |    0.02 | 0.0219 | 0.0086 |     - |     184 B |

        [Benchmark(Baseline = true)]
        public void EvaluateIsHomepageWithJavascript()
        {
            Convert.ToBoolean(_engine.Evaluate(_scope, "isHomepage()"));            
        }

        [Benchmark]
        public async Task EvaluateIsHomepageWithRule()
        {
            await _ruleService.EvaluateAsync(_rule);       
        }        
    }
}
