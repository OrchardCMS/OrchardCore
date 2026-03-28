using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Users;

public sealed class UserOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static UserOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineScript("password-generator")
            .SetUrl("~/OrchardCore.Users/Scripts/password-generator.min.js", "~/OrchardCore.Users/Scripts/password-generator.js")
            .SetVersion("1.0.0");

        s_manifest
            .DefineScript("qrcode")
            .SetUrl("~/OrchardCore.Users/Scripts/qrcode.min.js", "~/OrchardCore.Users/Scripts/qrcode.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("phone-input")
            .SetUrl("~/OrchardCore.Users/Scripts/phone-input.min.js", "~/OrchardCore.Users/Scripts/phone-input.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("phone-input")
            .SetUrl("~/OrchardCore.Users/Styles/phone-input.min.css", "~/OrchardCore.Users/Styles/phone-input.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
