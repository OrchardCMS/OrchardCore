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
    public async Task ProcessUserInfoTest()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var userProperties = @"{
            ""UserProfile"": {
                ""ContentItemId"": ""4fkpnj0fnawmzy3zdc5kx5pnn1"",
                ""ContentItemVersionId"": null,
                ""ContentType"": ""UserProfile"",
                ""DisplayText"": ""admin admin - (admin)"",
                ""Latest"": true,
                ""Published"": true,
                ""ModifiedUtc"": null,
                ""PublishedUtc"": null,
                ""CreatedUtc"": null,
                ""Owner"": ""4fkpnj0fnawmzy3zdc5kx5pnn1"",
                ""Author"": ""admin"",
                ""UserProfile"": {
                    ""UserName"": {
                        ""Text"": ""admin""
                    },
                    ""Email"": {
                        ""Text"": ""admin@admin.com""
                    },
                    ""OwnerUser"": {
                        ""UserIds"": [
                            ""4fkpnj0fnawmzy3zdc5kx5pnn1""
                        ],
                        ""UserNames"": []
                    },
                    ""DisplayName"": {
                        ""Text"": ""admin admin""
                    },
                    ""Department"": {
                        ""ContentItemIds"": []
                    },
                    ""Manager"": {
                        ""UserIds"": [],
                        ""UserNames"": []
                    },
                    ""FirstName"": {
                        ""Text"": null
                    },
                    ""LastName"": {
                        ""Text"": null
                    },
                    ""Country"": {
                        ""Text"": ""ID""
                    },
                    ""Countries"": {
                        ""Values"": []
                    }
                },
                ""TitlePart"": {
                    ""Title"": ""admin - (admin)""
                },
                ""DIndexPart"": {}
            },
            ""UserNotificationPreferencesPart"": {
                ""Methods"": [
                    ""Email""
                ],
                ""Optout"": []
            },
            ""UserTimeZone"": {
                ""TimeZoneId"": null
            }
        }";

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync("admin") as User;
            user.Properties = JObject.Parse(userProperties);
            await userManager.UpdateAsync(user);

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
                    jobjUser.Remove("properties");
                    return userInfo;
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
