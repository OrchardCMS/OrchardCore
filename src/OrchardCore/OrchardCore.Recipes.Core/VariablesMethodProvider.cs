using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes
{
    public class VariablesMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public VariablesMethodProvider(JObject variables)
        {
            _globalMethod = new GlobalMethod
            {
                Name = "variables",
                Method = serviceprovider => (Func<string, object>)(name =>
                {
                    var value = variables[name].Value<string>();

                    // Replace variable value while the result returns another script
                    while (value.StartsWith('[') && value.EndsWith(']'))
                    {
                        value = value.Trim('[', ']');
                        value = (ScriptingManager.Evaluate(value, null, null, null) ?? "").ToString();
                        variables[name] = new JValue(value);
                    }

                    return value;
                })
            };
        }

        public IScriptingManager ScriptingManager { get; set; }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
