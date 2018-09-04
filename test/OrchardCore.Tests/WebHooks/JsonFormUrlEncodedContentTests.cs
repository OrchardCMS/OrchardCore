using System;
using Newtonsoft.Json.Linq;
using OrchardCore.WebHooks.Services.Http;
using Xunit;

namespace OrchardCore.Tests.WebHooks
{
    public class JsonFormUrlEncodedContentTests
    {
        [Fact] 
        public void FlatObjectTests() 
        { 
            TestJsonObjectAndFormsEncodedConversion("a=1", new JObject { { "a", 1 } }); 
            TestJsonObjectAndFormsEncodedConversion("a=1", new JObject { { "a", "1" } }); 
 
            TestJsonObjectAndFormsEncodedConversion("a=1&b=2", new JObject { { "a", "1" }, { "b", "2" } }); 
            TestJsonObjectAndFormsEncodedConversion("a=1&b=2.5", new JObject { { "a", "1" }, { "b", "2.5" } }); 
        } 
 
        [Fact] 
        public void SimpleArrays() 
        { 
            TestJsonObjectAndFormsEncodedConversion("a[]=1&a[]=2", new JObject { { "a", new JArray("1", "2") } }); 
            TestJsonObjectAndFormsEncodedConversion("a[]=1&a[]=2&b=3", new JObject { { "a", new JArray("1", "2") }, { "b", "3" } }); 
        } 
 
        [Fact] 
        public void MultiDimArrays() 
        { 
            TestJsonObjectAndFormsEncodedConversion("a[0][0][]=1", (JObject)JToken.Parse("{\"a\":[[[\"1\"]]]}")); 
            TestJsonObjectAndFormsEncodedConversion("a[0][]=1&a[0][]=2&a[1][]=3&a[1][]=4", (JObject)JToken.Parse("{\"a\":[[1,2],[3,4]]}")); 
            TestJsonObjectAndFormsEncodedConversion("a[0][]=1&a[0][]=2&a[1][]=3&a[1][]=4", 
                (JObject)JToken.Parse("{'a':[['1','2'],['3','4']]}".Replace('\'', '\"'))); 
            TestJsonObjectAndFormsEncodedConversion("a[0][]=1&a[0][]=2&a[0][]=3&a[1][]=4", 
                (JObject)JToken.Parse("{'a':[['1','2','3'],['4']]}".Replace('\'', '\"'))); 
        } 
 
        [Fact] 
        public void DeepObjects() 
        { 
            TestJsonObjectAndFormsEncodedConversion("a[b]=1&a[c]=2&a[d][]=3&a[e][f]=4", 
                (JObject)JToken.Parse("{'a':{'b':'1','c':'2','d':['3'],'e':{'f':'4'}}}" 
                    .Replace('\'', '\"'))); 
        } 
 
        [Fact] 
        public void NullValues() 
        { 
            TestJsonObjectAndFormsEncodedConversion("a[b]=1&a[d][]=3", 
                (JObject)JToken.Parse("{'a':{'b':'1','c':null,'d':['3'],'e':{'f':null}}}" 
                    .Replace('\'', '\"'))); 
        } 
 
        void TestJsonObjectAndFormsEncodedConversion(string formUrlEncoded, JObject json) 
        { 
            var content = new JsonFormUrlEncodedContent(json);
            var output = content.ReadAsStringAsync().Result;

            Assert.Equal(formUrlEncoded, Uri.UnescapeDataString(output));
        } 
    }
}