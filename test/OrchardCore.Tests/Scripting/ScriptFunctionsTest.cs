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
                    return new JsonObject
                    {
                        ["name"] = "test-name"
                    }; ;
                }
            };

            var scriptingEngine = scope.ServiceProvider.GetRequiredService<IScriptingEngine>();
            var scriptingScope = scriptingEngine.CreateScope([findUser], scope.ServiceProvider, null, null);
            var result = (bool)scriptingEngine.Evaluate(scriptingScope, "var jsonObject = tryAccessJsonObject(); return jsonObject.name == 'test-name'");
            Assert.True(result);
            return Task.CompletedTask;
        });
    }
}
