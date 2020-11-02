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
        [Fact]
        public void CanAccessJObjectProperties()
        {
            var methods = new List<GlobalMethod>();

            methods.Add(new GlobalMethod(){
                Name= "GetJObject",
                Method = serviceProvider => (Func<JObject>)(() =>
                {
                    return new JObject
                    {
                        new JProperty("name", "test-name")
                    };
                })
            });
            var mockServiceProvider = new Mock<IServiceProvider>();
            var engine = new JavaScriptEngine(new MemoryCache(new MemoryCacheOptions()));
            var scope = engine.CreateScope(methods, mockServiceProvider.Object, null, null);

           var returnValue = engine.Evaluate(scope, "return GetJObject().name == 'test-name';");

           Assert.Equal(true, returnValue);
        }
    }
}
