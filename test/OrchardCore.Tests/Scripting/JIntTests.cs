using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using Xunit;

namespace OrchardCore.Tests.Html
{
    public class JintTests
    {

        private object Evaluate(IEnumerable<GlobalMethod> methods, string script)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var engine = new JavaScriptEngine(new MemoryCache(new MemoryCacheOptions()));
            var scope = engine.CreateScope(methods, mockServiceProvider.Object, null, null);
            return engine.Evaluate(scope, script);
        }
        [Fact]
        public void CanAccessPropertiesWithAccessor()
        {
            var methods = new List<GlobalMethod>();

            methods.Add(new GlobalMethod()
            {
                Name = "GetJObject",
                Method = serviceProvider => (Func<JObject>)(() =>
                {
                    return new JObject
                    {
                        new JProperty("name", "test-name")
                    };
                })
            });

            Assert.Equal(true, Evaluate(methods, "return GetJObject()['name'] == 'test-name';"));
        }
        [Fact]
        public void CanAccessPropertiesDotValue()
        {
            var methods = new List<GlobalMethod>();

            methods.Add(new GlobalMethod()
            {
                Name = "GetJObject",
                Method = serviceProvider => (Func<JObject>)(() =>
                {
                    return new JObject
                    {
                        new JProperty("name", "test-name")
                    };
                })
            });
            Assert.Equal(true, Evaluate(methods, "return GetJObject().name== 'test-name';"));
        }
    }
}
