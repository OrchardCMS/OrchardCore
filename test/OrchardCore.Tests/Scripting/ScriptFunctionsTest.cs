using System.Text.Json.Nodes;
using OrchardCore.Scripting;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Scripting;
public class ScriptFunctionsTest
{
    [Fact]
    public async Task TheScriptingEngineShouldBeAbleToHandleJsonObject()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(scope =>
        {
            var findUser = new GlobalMethod
            {
                Name = "tryAccessJsonObject",
                Method = sp => () =>
                {
                    const string jsonData = """
                    {
                        "age":33,
                        "falseValue":false,
                        "trueValue":true,
                        "stringValue":"stringTest",
                        "employees": {
                            "type": "array",
                            "value": [
                                {
                                    "firstName": "John",
                                    "lastName": "Doe"
                                },
                                {
                                    "firstName": "Jane",
                                    "lastName": "Doe"
                                }
                            ]
                        }
                    }
                    """;

                    return JObject.Parse(jsonData);
                }
            };

            var scriptingEngine = scope.ServiceProvider.GetRequiredService<IScriptingEngine>();
            var scriptingScope = scriptingEngine.CreateScope([findUser], scope.ServiceProvider, null, null);
            var result = (bool)scriptingEngine.Evaluate(scriptingScope,
                @"var jobj = tryAccessJsonObject();
                    return jobj.age == 33;
                ");
            Assert.True(result);

            result = (bool)scriptingEngine.Evaluate(scriptingScope,
                  @"var jobj = tryAccessJsonObject();
                    return jobj.stringValue == ""stringTest"";
                ");
            Assert.True(result);
            result = (bool)scriptingEngine.Evaluate(scriptingScope,
                        @"var jobj = tryAccessJsonObject();
                            return jobj.employees.type == ""array"" &&
                                    jobj.employees.value[0].firstName == ""John"";
                        ");
            Assert.True(result);


            var result1 = scriptingEngine.Evaluate(scriptingScope,
                  @"var jobj = tryAccessJsonObject();
                var steps = [];
                if(!jobj.falseValue) steps.push(1);
                if(jobj.trueValue) steps.push(2);

                // falseValue should be false 
                if(jobj.falseValue == false) steps.push(3);
                if(jobj.trueValue == true) steps.push(4);
                if(!!jobj.trueValue) steps.push(5);
                steps.push(jobj.falseValue);
                steps.push(jobj.falseValue.toString());

             return steps.join(',')
            ");
            Assert.Equal("1,2,3,4,5,false,false", result1);


            return Task.CompletedTask;
        });
    }
}
