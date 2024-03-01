using System.Text.Json;
using OrchardCore.Users.Core.Json;

namespace OrchardCore.Tests.Serializers;

public class JsonSerializerTests
{
    [Fact]
    public static void Deserialize_WhenCalled_DeserializeValidJson()
    {
        var json = "{\"LoginProvider\":\"OpenIdConnect\",\"ProviderKey\":\"abc\",\"ProviderDisplayName\":\"default\"}";

        var options = new JsonSerializerOptions();

        options.Converters.Add(new LoginInfoJsonConverter());

        var obj = JsonSerializer.Deserialize<UserLoginInfo>(json, options);

        Assert.Equal("OpenIdConnect", obj.LoginProvider);
        Assert.Equal("abc", obj.ProviderKey);
        Assert.Equal("default", obj.ProviderDisplayName);
    }
}
