using System.IO.Hashing;
using System.Text;
using OrchardCore.Facebook.Endpoints;

namespace OrchardCore.Facebook.Settings;

public class FacebookSettings
{
    public string AppId { get; set; }
    public string AppSecret { get; set; }
    public bool FBInit { get; set; }

    public string FBInitParams { get; set; } = """
        status: true,
        xfbml: true,
        autoLogAppEvents: true
        """;

    public string SdkJs { get; set; } = "sdk.js";
    public string Version { get; set; } = "v3.2";

    public ulong GetHash()
    {
        return XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(string.Concat(AppId, FBInit, FBInitParams, SdkJs, Version, GetSdkEndpoints.Version.ToString())));
    }
}
