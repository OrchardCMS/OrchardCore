using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Scripting;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Scripting;
public class ScriptFunctionsTest
{
    [Fact]
    public async Task UserShouldBeAllowedConvertedToJsonObjectAndUsedInScripts()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var findUser = new GlobalMethod
            {
                Name = "getUserByName",
                Method = sp => (string userName) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<IUser>>();
                    var userInfo = userManager.FindByNameAsync(userName).GetAwaiter().GetResult();
                    var jobjUser = JObject.FromObject(userInfo, JOptions.CamelCase);
                    jobjUser.Remove("securityStamp");
                    jobjUser.Remove("passwordHash");
                    jobjUser.Remove("resetToken");
                    jobjUser.Remove("userTokens");
                    return jobjUser;
                }
            };

            var scriptingEngine = scope.ServiceProvider.GetRequiredService<IScriptingEngine>();
            var scriptingScope = scriptingEngine.CreateScope([findUser], scope.ServiceProvider, null, null);
            var result = (bool)scriptingEngine.Evaluate(scriptingScope, "var user = getUserByName('admin'); return user.userName == 'admin'");
            Assert.True(result);
            var result1 = scriptingEngine.Evaluate(scriptingScope, "var user2 = getUserByName('admin'); return user2.userName");
            Assert.NotNull(result1);
        });
    }
}
