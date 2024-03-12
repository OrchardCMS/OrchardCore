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
                        "booleanValue":false,
                        "booleanValue1":true,
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

            var result1 = scriptingEngine.Evaluate(scriptingScope,
                  @"var jobj = tryAccessJsonObject();
                var steps = [];
                
                if(!jobj.booleanValue) steps.push(1);

                // booleanValue should be false 
                if(jobj.booleanValue == false) steps.push(2);
                if(jobj.booleanValue1 == true) steps.push(3);
                if(!jobj.booleanValue) steps.push(4);
                if(!!jobj.booleanValue1) steps.push(5);
                steps.push(jobj.booleanValue);
                steps.push(jobj.booleanValue.toString());

             return steps.join(',')
            ");
            Assert.Equal("1,2,3,4,5,false,false", result1);

            result = (bool)scriptingEngine.Evaluate(scriptingScope,
            @"var jobj = tryAccessJsonObject();
                            return jobj.employees.type == ""array"" &&
                                    jobj.employees.value[0].firstName == ""John"";
                        ");
            Assert.True(result);
            return Task.CompletedTask;
        });
    }
}
