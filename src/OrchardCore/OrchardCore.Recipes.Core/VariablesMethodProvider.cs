using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes
{
    public class VariablesMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;
        private const string GlobalMethodName = "variables";

        public VariablesMethodProvider(JObject variables, List<IGlobalMethodProvider> scopedMethodProviders)
        {
            _globalMethod = new GlobalMethod
            {
                Name = GlobalMethodName,
                Method = serviceProvider => (Func<string, object>)(name =>
                {
                    var variable = variables[name];

                    if (variable == null)
                    {
                        var S = serviceProvider.GetService<IStringLocalizer<VariablesMethodProvider>>();

                        throw new ValidationException(S["The variable '{0}' was used in the recipe but not defined. Make sure you add the '{0}' variable in the '{1}' section of the recipe.", name, GlobalMethodName]);
                    }

                    var value = variable.Value<string>();

                    // Replace variable value while the result returns another script.
                    while (value.StartsWith('[') && value.EndsWith(']'))
                    {
                        value = value.Trim('[', ']');
                        value = (ScriptingManager.Evaluate(value, null, null, scopedMethodProviders) ?? "").ToString();
                        variables[name] = new JValue(value);
                    }

                    return value;
                }),
            };
        }

        public static IScriptingManager ScriptingManager => ShellScope.Services.GetRequiredService<IScriptingManager>();

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
