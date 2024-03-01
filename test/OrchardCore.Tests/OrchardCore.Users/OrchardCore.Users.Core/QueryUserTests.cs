using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.OrchardCore.Users.OrchardCore.Users.Core;
public class QueryUserTests
{
    [Fact]
    public async Task QueryUserLoginInfoTest()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
         {
             var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

             var user = await userManager.FindByNameAsync("admin") as User;
             var loginInfo = new UserLoginInfo("test", "test", "test");
             user.LoginInfos = new List<UserLoginInfo>() {
               loginInfo
             };

             await userManager.UpdateAsync(user);

             var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

             var dbUser = await session.Query<User, UserIndex>(x => x.NormalizedUserName == userManager.NormalizeName("admin"))
                    .FirstOrDefaultAsync();
             Assert.NotNull(dbUser);
             var loginInfo1 = dbUser.LoginInfos.FirstOrDefault();
             Assert.NotNull(loginInfo);
             Assert.Equal(loginInfo.ProviderKey, loginInfo1.ProviderKey);

         });
    }

}
