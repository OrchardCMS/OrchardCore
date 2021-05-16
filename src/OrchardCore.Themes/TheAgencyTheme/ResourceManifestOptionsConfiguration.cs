using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAgencyTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheAgencyTheme-jQuery")
                .SetCdn("https://code.jquery.com/jquery-3.5.1.min.js", "https://code.jquery.com/jquery-3.5.1.js")
                .SetCdnIntegrity("sha384-ZvpUoO/+PpLXR1lu4jmpXWu80pZlYUAfxl5NsBMWOEPSjUn/6Z/hRTt8+pR6L4N2", "sha384-/LjQZzcpTzaYn7qWqRIWYC5l8FWEZ2bIHIz0D73Uzba4pShEcdLdZyZkI4Kv676E")
                .SetVersion("3.5.1");

            _manifest
                .DefineScript("TheAgencyTheme-bootstrap-bundle")
                .SetDependencies("TheAgencyTheme-jQuery")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.6.0/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.6.0/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-Piv4xVNRyMGpqkS2by6br4gNJ7DXjqk09RmUpJ8jgGtD7zP9yug3goQfGII0yAns", "sha384-ZEiV3j24mJ99uunGq2LXkicfbHO18KBrDHpdTjD1NUT07kWB6B8u6U5t6UBimJ7w")
                .SetVersion("4.6.0");

            _manifest
                .DefineScript("anime")
                .SetDependencies("TheAgencyTheme-jQuery")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/animejs/3.2.1/anime.min.js")
                .SetCdnIntegrity("sha384-fXdIufVbE9aU7STmdk/DWK0imNOozId9fTwzM/gi0NfPjphEIC3gq0M760UnsKVy")
                .SetVersion("3.2.1");

            _manifest
                .DefineScript("TheAgencyTheme")
                .SetDependencies("anime")
                .SetUrl("~/TheAgencyTheme/js/scripts.min.js", "~/TheAgencyTheme/js/scripts.js")
                .SetVersion("6.0.5");

            _manifest
                .DefineStyle("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/css/styles.min.css", "~/TheAgencyTheme/css/styles.css")
                .SetVersion("6.0.5");

            _manifest
                .DefineStyle("TheAgencyTheme-bootstrap-oc")
                .SetDependencies("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/css/bootstrap-oc.min.css", "~/TheAgencyTheme/css/bootstrap-oc.css")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
