using System.Text.Json.Nodes;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes;

public class ParametersMethodProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _globalMethod;

    public ParametersMethodProvider(object environment)
    {
        var environmentObject = JObject.FromObject(environment);

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
