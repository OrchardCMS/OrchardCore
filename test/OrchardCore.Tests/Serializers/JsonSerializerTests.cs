using System.Text.Json;
using OrchardCore.ContentFields.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users.Core.Json;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Serializers;

public class JsonSerializerTests
{
    private const string _userLoginInfo = "{\"LoginProvider\":\"OpenIdConnect\",\"ProviderKey\":\"abc\",\"ProviderDisplayName\":\"default\"}";

    private readonly JsonSerializerOptions _options;

    public JsonSerializerTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new LoginInfoJsonConverter());
    }

    [Fact]
    public void Deserialize_WhenCalled_ReturnValidUserLoginInfo()
    {
        var obj = JsonSerializer.Deserialize<UserLoginInfo>(_userLoginInfo, _options);

        Assert.Equal("OpenIdConnect", obj.LoginProvider);
        Assert.Equal("abc", obj.ProviderKey);
        Assert.Equal("default", obj.ProviderDisplayName);
    }

    [Theory]
    [InlineData("{\"name\":\"One\",\"value\":\"1\",\"Weight\":\"1.75\"}", 1.75)]
    [InlineData("{\"name\":\"One\",\"value\":\"1\",\"Weight\":\"1\"}", 1)]
    [InlineData("{\"name\":\"One\",\"value\":\"1\",\"Weight\":1}", 1)]
    [InlineData("{\"name\":\"One\",\"value\":\"1\",\"Weight\":2.75}", 2.75)]
    public void Deserialize_WhenCalled_ReturnDoubleFromStringWithBaseOptions(string json, double expectedWeight)
    {
        var item = JsonSerializer.Deserialize<CustomListValueOption>(json, JOptions.Base);

        Assert.Equal(expectedWeight, item.Weight);
    }

    [Fact]
    public void Serialize_WhenCalled_ReturnValidJson()
    {
        var loginInfo = new UserLoginInfo("OpenIdConnect", "abc", "default");
        var json = JsonSerializer.Serialize(loginInfo, _options);

        Assert.Equal(_userLoginInfo, json);
    }

    [Fact]
    public async Task DefaultContentSerializer_SerializeAndDeserialize_UserWithUserLoginInfo()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var loginInfo = new UserLoginInfo("OpenIdConnect", "abc", "default");

            var newUser = new User()
            {
                UserId = "abc",
                UserName = "mike",
                Email = "test@test.com",
                LoginInfos =
                [
                    loginInfo
                ]
            };

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();

            await session.SaveAsync(newUser);
            await session.SaveChangesAsync();

            var dbUser = await session.Query<User, UserIndex>(x => x.UserId == "abc").FirstOrDefaultAsync();

            Assert.NotNull(dbUser);

            var userLoginInfo = dbUser.LoginInfos.FirstOrDefault();

            Assert.NotNull(userLoginInfo);
            Assert.Equal(loginInfo.LoginProvider, userLoginInfo.LoginProvider);
            Assert.Equal(loginInfo.ProviderKey, userLoginInfo.ProviderKey);
            Assert.Equal(loginInfo.ProviderDisplayName, userLoginInfo.ProviderDisplayName);
        });
    }

    public sealed class CustomListValueOption : ListValueOption
    {
        public double? Weight { get; set; }
    }
}
