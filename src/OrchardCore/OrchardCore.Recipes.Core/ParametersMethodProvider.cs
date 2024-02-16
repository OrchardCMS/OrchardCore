using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes
{
    public class ParametersMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public ParametersMethodProvider(object environment, JsonSerializerOptions jsonSerializerOptions)
        {
            var environmentObject = JObject.FromObject(environment, jsonSerializerOptions);

            _globalMethod = new GlobalMethod
            {
                Name = "parameters",
                Method = serviceprovider => (Func<string, object>)(name =>
               {
                   return environmentObject.SelectNode(name)?.Value<string>();
               }),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
