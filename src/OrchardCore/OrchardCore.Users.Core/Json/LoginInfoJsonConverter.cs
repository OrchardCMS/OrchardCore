using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Core.Json;

public class LoginInfoJsonConverter : JsonConverter<UserLoginInfo>
{
    public static readonly LoginInfoJsonConverter Instance = new();

    public override UserLoginInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var loginInfo = new UserLoginInfo(string.Empty, string.Empty, string.Empty);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(UserLoginInfo.LoginProvider):
                        loginInfo.LoginProvider = reader.GetString();
                        break;
                    case nameof(UserLoginInfo.ProviderKey):
                        loginInfo.ProviderKey = reader.GetString();
                        break;
                    case nameof(UserLoginInfo.ProviderDisplayName):
                        loginInfo.ProviderDisplayName = reader.GetString();
                        break;
                    default:
                        break;
                }
            }
        }

        return loginInfo;
    }

    public override void Write(Utf8JsonWriter writer, UserLoginInfo objectToWrite, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(UserLoginInfo.LoginProvider), objectToWrite.LoginProvider);
        writer.WriteString(nameof(UserLoginInfo.ProviderKey), objectToWrite.ProviderKey);
        writer.WriteString(nameof(UserLoginInfo.ProviderDisplayName), objectToWrite.ProviderDisplayName);
        writer.WriteEndObject();
    }
}
