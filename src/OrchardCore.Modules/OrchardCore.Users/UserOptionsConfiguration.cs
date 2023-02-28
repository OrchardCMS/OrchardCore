using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Users
{
    public class UserOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static UserOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("password-generator")
                .SetUrl("~/OrchardCore.Users/Scripts/password-generator.min.js", "~/OrchardCore.Users/Scripts/password-generator.js")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
