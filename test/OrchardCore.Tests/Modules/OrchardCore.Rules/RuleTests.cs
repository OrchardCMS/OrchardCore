using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Localization;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Rules
{
    public class RuleTests
    {
        [Fact]
        public async Task ShouldEvaluateRuleFalseWhenNoConditions()
        {
            var rule = new Rule();

            var services = CreateServiceCollection();

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
                Conditions = new List<Condition>
                {
                    new HomepageCondition 
                    { 
                        Value = isHomepage 
                    }
                }
            };

            var services = CreateServiceCollection()
                .AddCondition<HomepageCondition, HomepageConditionEvaluator, ConditionFactory<HomepageCondition>>();

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
                Conditions = new List<Condition>
                {
                    new BooleanCondition { Value = boolean }
                }
            };

            var services = CreateServiceCollection()
                .AddCondition<BooleanCondition, BooleanConditionEvaluator, ConditionFactory<BooleanCondition>>();

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
                Conditions = new List<Condition>
                {
                    new AnyConditionGroup
                    {
                        Conditions = new List<Condition>
                        {
                            new BooleanCondition { Value = first },
                            new BooleanCondition { Value = second }
                        }
                    }
                }
            };

            var services = CreateServiceCollection()
                .AddCondition<AnyConditionGroup, AnyConditionEvaluator, ConditionFactory<AnyConditionGroup>>()
                .AddCondition<BooleanCondition, BooleanConditionEvaluator, ConditionFactory<BooleanCondition>>();

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
                Conditions = new List<Condition>
                {
                    new UrlCondition
                    {
                        Value = path,
                        Operation = new StringEqualsOperator()
                    }
                }
            };

            var services = CreateServiceCollection()
                .AddCondition<UrlCondition, UrlConditionEvaluator, ConditionFactory<UrlCondition>>();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString(requestPath);
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            services.AddSingleton<IHttpContextAccessor>(mockHttpContextAccessor.Object);

            var serviceProvider = services.BuildServiceProvider();

            var ruleService = serviceProvider.GetRequiredService<IRuleService>();

            Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
        }

        private ServiceCollection CreateServiceCollection()
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
}
