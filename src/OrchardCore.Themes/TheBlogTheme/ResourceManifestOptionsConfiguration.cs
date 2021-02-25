using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheBlogTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheBlogTheme-vendor-jQuery")
                .SetUrl("~/TheBlogTheme/vendor/jquery/jquery.min.js", "~/TheBlogTheme/vendor/jquery/jquery.js")
                .SetCdn("https://code.jquery.com/jquery-3.5.1.min.js", "https://code.jquery.com/jquery-3.5.1.js")
                .SetCdnIntegrity("sha384-ZvpUoO/+PpLXR1lu4jmpXWu80pZlYUAfxl5NsBMWOEPSjUn/6Z/hRTt8+pR6L4N2", "sha384-/LjQZzcpTzaYn7qWqRIWYC5l8FWEZ2bIHIz0D73Uzba4pShEcdLdZyZkI4Kv676E")
                .SetVersion("3.5.1");

            _manifest
                .DefineScript("TheBlogTheme-vendor-jQuery.slim")
                .SetUrl("~/TheBlogTheme/vendor/jquery/jquery.slim.min.js", "~/TheBlogTheme/vendor/jquery/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.5.1.slim.min.js", "https://code.jquery.com/jquery-3.5.1.slim.js")
                .SetCdnIntegrity("sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj", "sha384-x6NENSfxadikq2gB4e6/qompriNc+y1J3eqWg3hAAMNBs4dFU303XMTcU3uExJgZ")
                .SetVersion("3.5.1");


            _manifest
                .DefineScript("TheBlogTheme-vendor-bootstrap")
                .SetDependencies("TheBlogTheme-vendor-jQuery")
                .SetUrl("~/TheBlogTheme/vendor/bootstrap/js/bootstrap.min.js", "~/TheBlogTheme/vendor/bootstrap/js/bootstrap.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.js")
                .SetCdnIntegrity("sha384-w1Q4orYjBQndcko6MimVbzY0tgp4pWB4lZ7lr30WKz0vr/aWKhXdBNmNb5D92v7s", "sha384-UACdLYI/ku0J5/hhojLBvchW9KNT++8VmZQW9cJDVIEth0APvgNO4qSPpIxp8F9T")
                .SetVersion("4.5.3");

            _manifest
                .DefineScript("TheBlogTheme-vendor-bootstrap-bundle")
                .SetDependencies("TheBlogTheme-vendor-jQuery")
                .SetUrl("~/TheBlogTheme/vendor/bootstrap/js/bootstrap.bundle.min.js", "~/TheBlogTheme/vendor/bootstrap/js/bootstrap.bundle.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-ho+j7jyWK8fNQe+A12Hb8AhRq26LrZ/JpcUGGOn+Y7RsweNrtN/tE3MoK7ZeZDyx", "sha384-q/QDYob/o4XN/fpFWt7AEU+hWQLAc0FCviSzMpKqarlw2VVqk2mgn971KCJ64zpo")
                .SetVersion("4.5.3");

            _manifest
                .DefineStyle("TheBlogTheme-vendor-bootstrap")
                .SetUrl("~/TheBlogTheme/vendor/bootstrap/css/bootstrap.min.css", "~/TheBlogTheme/vendor/bootstrap/css/bootstrap.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.css")
                .SetCdnIntegrity("sha384-TX8t27EcRE3e/ihU7zmQxVncDAy5uIKz4rEkgIXeMed4M0jlfIDPvg6uqKI2xXr2", "sha384-Ro2DNoUODgrLmRM7WL/mbXZ1D6WaudEiPPceIZTfzZrTahyJAxLMj5TF2RQwrpiG")
                .SetVersion("4.5.3");

            _manifest
                .DefineStyle("TheBlogTheme-bootstrap-oc")
                .SetUrl("~/TheBlogTheme/css/bootstrap-oc.min.css", "~/TheBlogTheme/css/bootstrap-oc.css")
                .SetVersion("1.0.0");

            _manifest
                .DefineStyle("TheBlogTheme-vendor-font-awesome")
                .SetUrl("~/TheBlogTheme/vendor/fontawesome-free/css/all.min.css", "~/TheBlogTheme/vendor/fontawesome-free/css/all.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/css/all.css")
                .SetCdnIntegrity("sha384-vp86vTRFVJgpjF9jiIGPEEqYqlDwgyBgEF109VFjmqGmIY/Y4HV4d3Gp2irVfcrp", "sha384-onlHyXkx02a/ZTmCZVUQDO+4JnJwhOvHXBLvN1aal24ABfJ0VDUt7RyzsmpPswfe")
                .SetVersion("5.15.1");

            _manifest
                .DefineScript("TheBlogTheme-vendor-font-awesome")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/js/all.js")
                .SetCdnIntegrity("sha384-9/D4ECZvKMVEJ9Bhr3ZnUAF+Ahlagp1cyPC7h5yDlZdXs4DQ/vRftzfd+2uFUuqS", "sha384-vkhNV6UI0lzGY+LU1vEFOQTnBcvaEr/ExwLUaZJmpqsmRZu5LDxNB+yEQiXIqCg9")
                .SetVersion("5.15.1");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
