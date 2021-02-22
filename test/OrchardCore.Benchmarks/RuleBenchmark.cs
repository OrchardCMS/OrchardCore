using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Benchmark.Support;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Mvc;
using OrchardCore.ResourceManagement;
using OrchardCore.Layers;
using OrchardCore.Layers.Services;
using OrchardCore.Localization;
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

// Summary 21st Feb 2021

// BenchmarkDotNet=v0.12.1, OS=macOS Catalina 10.15.7 (19H15) [Darwin 19.6.0]
// Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
// .NET Core SDK=5.0.103
//   [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
//   DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


// |                           Method |       Mean |    Error |   StdDev | Ratio |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
// |--------------------------------- |-----------:|---------:|---------:|------:|-------:|-------:|------:|----------:|
// | EvaluateIsHomepageWithJavascript | 2,397.3 ns | 31.24 ns | 27.69 ns |  1.00 | 0.2365 | 0.0763 |     - |    1168 B |
// |       EvaluateIsHomepageWithRule |   880.8 ns | 16.97 ns | 22.07 ns |  0.37 | 0.0343 | 0.0095 |     - |     224 B |

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
