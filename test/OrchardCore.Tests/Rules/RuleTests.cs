using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using Xunit;

namespace OrchardCore.Tests.Rules
{
    public class RuleTests
    {
        [Fact]
        public async Task ShouldEvaluateFalseWhenNoChildren()
        {
            var rule = new AllRule();

            var services = CreateServiceCollection();

            var serviceProvider = services.BuildServiceProvider();

            var ruleService = serviceProvider.GetRequiredService<IRuleService>();

            Assert.False(await ruleService.EvaluateAsync(rule));
        }        

        [Theory]
        [InlineData("/", true)]
        [InlineData("/notthehomepage", false)]
        public async Task ShouldEvaluateIsHomepage(string path, bool expected)
        {
            var rule = new AllRule
            {
                Children = new[]
                {
                    new IsHomepageRule()
                }
            };

            var services = CreateServiceCollection()
                .AddRuleMethod<IsHomepageRule, IsHomepageRuleEvaluator>();

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
            var rule = new AllRule
            {
                Children = new[]
                {
                    new BooleanRule { Value = boolean }
                }
            };

            var services = CreateServiceCollection()
                .AddRuleMethod<BooleanRule, BooleanRuleEvaluator>();

            var serviceProvider = services.BuildServiceProvider();

            var ruleService = serviceProvider.GetRequiredService<IRuleService>();

            Assert.Equal(expected, await ruleService.EvaluateAsync(rule));
        }

        [Theory]
        [InlineData("/foo", "/foo", true)]
        [InlineData("/bar", "/foo", false)]
        public async Task ShouldEvaluateUrlEquals(string path, string requestPath, bool expected)
        {
            var rule = new AllRule
            {
                Children = new[]
                {
                    new UrlRule 
                    {
                        Value = path,
                        Operation = new StringEqualsOperator()
                    }
                }
            };

            var services = CreateServiceCollection()
                .AddRuleMethod<UrlRule, UrlRuleEvaluator>()
                .AddRuleOperator<StringEqualsOperator, StringEqualsOperatorComparer>();

            services.Configure<RuleOptions>(o => 
            {
                // This looks like enough to describe.
                // it'll just have to be an overload that takes S
                o.Operators = new List<OperatorOption>
                {
                    new OperatorOption
                    {
                        DisplayText = "Equals",
                        Operator = typeof(StringEqualsOperator),
                        Comparer = new StringEqualsOperatorComparer()
                    }
                };


                // Don't think we need this at all.
                // the string operator parameter is enough to know that it takes string operators.
                // operators will just need localizing.
                // o.MethodOptions.Add(new MethodOption
                // {
                //     Type = typeof(UrlMethod),
                //     Operators = new List<OperatorOption>
                //     {
                //         new OperatorOption
                //         {
                //             Type = typeof()

                //         }
                //     }

                // });

            });




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
            services.AddOptions<RuleOptions>();

            services.AddRuleMethod<AllRule, AllRuleEvaluator>();

            services.AddTransient<IRuleResolver, RuleResolver>();
            services.AddTransient<IOperatorResolver, OperatorResolver>();

            services.AddTransient<IRuleService, RuleService>();

            services.AddTransient<AllRuleEvaluator>();

            return services;
        }
    }
}