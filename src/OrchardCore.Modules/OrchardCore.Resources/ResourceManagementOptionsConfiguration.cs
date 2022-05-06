using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private readonly ISiteService _siteService;
        private readonly IHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _pathBase;

        private const string cloudflareUrl = "https://cdnjs.cloudflare.com/ajax/libs/";
        // Versions
        private const string codeMirrorVersion = "5.65.3";
        private const string monacoEditorVersion = "0.33.0";
        // URLs
        private const string codeMirrorUrl = cloudflareUrl + "codemirror/" + codeMirrorVersion + "/";

        public ResourceManagementOptionsConfiguration(ISiteService siteService, IHostEnvironment env, IHttpContextAccessor httpContextAccessor, ShellSettings shellSettings)
        {
            _siteService = siteService;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _pathBase = _httpContextAccessor.HttpContext.Request.PathBase;
        }

        ResourceManifest BuildManifest()
        {
            var manifest = new ResourceManifest();

            manifest
                .DefineScript("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.min.js", "~/OrchardCore.Resources/Scripts/jquery.js")
                .SetCdn("https://code.jquery.com/jquery-3.6.0.min.js", "https://code.jquery.com/jquery-3.6.0.js")
                .SetCdnIntegrity("sha384-vtXRMe3mGCbOeY7l30aIg8H9p3GdeSe4IFlP6G8JMa7o7lXvnz3GFKzPxzJdPfGK", "sha384-S58meLBGKxIiQmJ/pJ8ilvFUcGcqgla+mWH9EEKGm6i6rKxSTA2kpXJQJ8n7XK4w")
                .SetVersion("3.6.0");

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.slim.min.js", "~/OrchardCore.Resources/Scripts/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.6.0.slim.min.js", "https://code.jquery.com/jquery-3.6.0.slim.js")
                .SetCdnIntegrity("sha384-Qg00WFl9r0Xr6rUqNLv1ffTSSKEFFCDCKVyHZ+sVt8KuvG99nWw5RNvbhuKgif9z", "sha384-fuUlMletgG/KCb0NwIZTW6aMv/YBbXe0Wt71nwLRreZZpesG/N/aURjEZCG6mtYn")
                .SetVersion("3.6.0");

            manifest
                .DefineScript("jQuery")
                .SetUrl("~/OrchardCore.Resources/Vendor/jquery-3.5.1/jquery.min.js", "~/OrchardCore.Resources/Vendor/jquery-3.5.1/jquery.js")
                .SetCdn("https://code.jquery.com/jquery-3.5.1.min.js", "https://code.jquery.com/jquery-3.5.1.js")
                .SetCdnIntegrity("sha384-ZvpUoO/+PpLXR1lu4jmpXWu80pZlYUAfxl5NsBMWOEPSjUn/6Z/hRTt8+pR6L4N2", "sha384-/LjQZzcpTzaYn7qWqRIWYC5l8FWEZ2bIHIz0D73Uzba4pShEcdLdZyZkI4Kv676E")
                .SetVersion("3.5.1");

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("~/OrchardCore.Resources/Vendor/jquery-3.5.1/jquery.slim.min.js", "~/OrchardCore.Resources/Vendor/jquery-3.5.1/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.5.1.slim.min.js", "https://code.jquery.com/jquery-3.5.1.slim.js")
                .SetCdnIntegrity("sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj", "sha384-x6NENSfxadikq2gB4e6/qompriNc+y1J3eqWg3hAAMNBs4dFU303XMTcU3uExJgZ")
                .SetVersion("3.5.1");

            manifest
                .DefineScript("jQuery")
                .SetUrl("~/OrchardCore.Resources/Vendor/jquery-3.4.1/jquery.min.js", "~/OrchardCore.Resources/Vendor/jquery-3.4.1/jquery.js")
                .SetCdn("https://code.jquery.com/jquery-3.4.1.min.js", "https://code.jquery.com/jquery-3.4.1.js")
                .SetCdnIntegrity("sha384-vk5WoKIaW/vJyUAd9n/wmopsmNhiy+L2Z+SBxGYnUkunIxVxAv/UtMOhba/xskxh", "sha384-mlceH9HlqLp7GMKHrj5Ara1+LvdTZVMx4S1U43/NxCvAkzIo8WJ0FE7duLel3wVo")
                .SetVersion("3.4.1");

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("~/OrchardCore.Resources/Vendor/jquery-3.4.1/jquery.slim.min.js", "~/OrchardCore.Resources/Vendor/jquery-3.4.1/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.4.1.slim.min.js", "https://code.jquery.com/jquery-3.4.1.slim.js")
                .SetCdnIntegrity("sha384-J6qa4849blE2+poT4WnyKhv5vZF5SrPo0iEjwBvKU7imGFAV0wwj1yYfoRSJoZ+n", "sha384-teRaFq/YbXOM/9FZ1qTavgUgTagWUPsk6xapwcjkrkBHoWvKdZZuAeV8hhaykl+G")
                .SetVersion("3.4.1");

            manifest
                .DefineScript("jQuery.easing")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.easing.min.js", "~/OrchardCore.Resources/Scripts/jquery.easing.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/jquery.easing@1.4.1/jquery.easing.min.js", "https://cdn.jsdelivr.net/npm/jquery.easing@1.4.1/jquery.easing.js")
                .SetCdnIntegrity("sha384-leGYpHE9Tc4N9OwRd98xg6YFpB9shlc/RkilpFi0ljr3QD4tFoFptZvgnnzzwG4Q", "sha384-fwPA0FyfPOiDsglgAC4ZWmBGwpXSZNkq9IG+cM9HL4CkpNQo4xgCDkOIPdWypLMX")
                .SetVersion("1.4.1");

            manifest
                .DefineScript("jQuery-ui")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-ui.min.js", "~/OrchardCore.Resources/Scripts/jquery-ui.js")
                .SetCdn("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js", "https://code.jquery.com/ui/1.12.1/jquery-ui.js")
                .SetCdnIntegrity("sha384-Dziy8F2VlJQLMShA6FHWNul/veM9bCkRUaLqr199K94ntO5QUrLJBEbYegdSkkqX", "sha384-JPbtLYL10d/Z1crlc6GGGGM3PavCzzoUJ1UxH0bXHOfguWHQ6XAWrIzW+MBGGXe5")
                .SetVersion("1.12.1");

            manifest
                .DefineStyle("jQuery-ui")
                .SetUrl("~/OrchardCore.Resources/Styles/jquery-ui.min.css", "~/OrchardCore.Resources/Styles/jquery-ui.css")
                .SetCdn("https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.min.css", "https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css")
                .SetCdnIntegrity("sha384-kcAOn9fN4XSd+TGsNu2OQKSuV5ngOwt7tg73O4EpaD91QXvrfgvf0MR7/2dUjoI6", "sha384-xewr6kSkq3dBbEtB6Z/3oFZmknWn7nHqhLVLrYgzEFRbU/DHSxW7K3B44yWUN60D")
                .SetVersion("1.12.1");

            manifest
                .DefineScript("jQuery-ui-i18n")
                .SetDependencies("jQuery-ui")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-ui-i18n.min.js", "~/OrchardCore.Resources/Scripts/jquery-ui-i18n.js")
                .SetCdn("https://code.jquery.com/ui/1.7.2/i18n/jquery-ui-i18n.min.js", "https://code.jquery.com/ui/1.7.2/i18n/jquery-ui-i18n.min.js")
                .SetCdnIntegrity("sha384-0rV7y4NH7acVmq+7Y9GM6evymvReojk9li+7BYb/ug61uqPSsXJ4uIScVY+N9qtd", "sha384-0rV7y4NH7acVmq+7Y9GM6evymvReojk9li+7BYb/ug61uqPSsXJ4uIScVY+N9qtd")
                .SetVersion("1.7.2");

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/js/bootstrap.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/js/bootstrap.js")
                .SetCdnIntegrity("sha384-vhJnz1OVIdLktyixHY4Uk3OHEwdQqPppqYR8+5mjsauETgLOcEynD9oPHhhz18Nw", "sha384-it0Suwx+VjMafDIVf5t+ozEbrflmNjEddSX5LstI/Xdw3nv4qP/a4e8K4k5hH6l4")
                .SetVersion("3.4.0");

            manifest
                .DefineStyle("bootstrap")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap.min.css", "https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap.css")
                .SetCdnIntegrity("sha384-PmY9l28YgO4JwMKbTvgaS7XNZJ30MK9FAZjjzXtlqyZCqBY6X6bXIkM++IkyinN+", "sha384-/5bQ8UYbZnrNY3Mfy6zo9QLgIQD/0CximLKk733r8/pQnXn2mgvhvKhcy43gZtJV")
                .SetVersion("3.4.0");

            manifest
                .DefineStyle("bootstrap-theme")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap-theme.min.css", "https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap-theme.css")
                .SetCdnIntegrity("sha384-jzngWsPS6op3fgRCDTESqrEJwRKck+CILhJVO5VvaAZCq8JYf8HsR/HPpBOOPZfR", "sha384-RtiWe5OsslAYZ9AVyorBziI2VQL7E27rzWygBJh7wrZuVPyK5jeQLLytnJIpJqfD")
                .SetVersion("3.4.0");

            manifest
                .DefineScript("popper")
                .SetUrl("~/OrchardCore.Resources/Vendor/popper-1.16.1/popper.min.js", "~/OrchardCore.Resources/Vendor/popper-1.16.1/popper.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.min.js", "https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.js")
                .SetCdnIntegrity("sha384-9/reFTGAW83EW2RDu2S0VKaIzap3H66lZH81PoYlFhbGU+6BZp6G7niu735Sk7lN", "sha384-cpSm/ilDFOWiMuF2bj03ZzJinb48NO9IGCXcYDtUzdP5y64Ober65chnoOj1XFoA")
                .SetVersion("1.16.1");

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery", "popper")
                .SetUrl("~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.min.js", "~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.js")
                .SetCdnIntegrity("sha256-SyTu6CwrfOhaznYZPoolVw2rxoY7lKYKQvqbtqN93HI=", "sha256-6+FBBJrY8QbYNs6AeCP3JSnzmmTX/+YFtN4iSOtYSKY=")
                .SetVersion("4.6.1");

            manifest
                .DefineScript("bootstrap-bundle")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.bundle.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha256-fgLAgv7fyCGopR/gBNq2iW3ZKIdqIcyshnUULC4vex8=", "sha256-eKb5bRTtGi7f8XfWkjxVGyJWtw9gS1X+9yqhNHklfWI=")
                .SetVersion("4.6.1");

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.min.css", "~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.css")
                .SetCdnIntegrity("sha256-DF7Zhf293AJxJNTmh5zhoYYIMs2oXitRfBjY+9L//AY=", "sha256-YQxBfLfP0/QyffXZNTDFES5IFXrxv+hYE9b2NK5TGcw=")
                .SetVersion("4.6.1");

            manifest
               .DefineScript("popperjs")
               .SetUrl("~/OrchardCore.Resources/Scripts/popper.min.js", "~/OrchardCore.Resources/Scripts/popper.js")
               .SetCdn("https://cdn.jsdelivr.net/npm/@popperjs/core@2.10.2/dist/umd/popper.min.js", "https://cdn.jsdelivr.net/npm/@popperjs/core@2.10.2/dist/umd/popper.js")
               .SetCdnIntegrity("sha384-7+zCNj/IqJ95wo16oMtfsKbZ9ccEh31eOz1HGyDuCQ6wgnyJNSYdrPa03rtR1zdB", "sha384-7bLhHCLchQRw474eiNFHP0txk38fyqbVOG/RohbcYXnTrdd9mNPQrVkOcY14iscj")
               .SetVersion("2.10.2");

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("popperjs")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.js")
                .SetCdnIntegrity("sha384-QJHtvGhmr9XOIpI6YVutG+2QOK9T+ZnN4kzFN1RtK3zEFEIsxhlmWl5/YESvpZ13", "sha384-4S2sRpwEfE5rUaiRVP4sETrP8WMo4pOHkAoMmvuju/2ycHM/QW1J7YQOjrPNpd5h")
                .SetVersion("5.1.3");

            manifest
                .DefineScript("bootstrap-bundle")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.bundle.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p", "sha384-8fq7CZc5BnER+jVlJI2Jafpbn4A9320TKhNJfYP33nneHep7sUg/OD30x7fK09pS")
                .SetVersion("5.1.3");

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.min.css", "~/OrchardCore.Resources/Styles/bootstrap.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.css")
                .SetCdnIntegrity("sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3", "sha384-BNKxVsQUv1K6rV5WJumgbzW1t/UsyFWm/uP4I8rr9T3PeBkcRGGApJv+rOjBpEdF")
                .SetVersion("5.1.3");

            manifest
                .DefineStyle("bootstrap-select")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap-select.min.css", "~/OrchardCore.Resources/Styles/bootstrap-select.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta2/dist/css/bootstrap-select.min.css", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta2/dist/css/bootstrap-select.css")
                .SetCdnIntegrity("sha256-UqiEyrW1sB5d6ZDzcWXKfYCR4MKVYMEdXNjJde84cjc=", "sha256-bmB8s0iyqUelK/WUqWRUG6y8K6RZYD2D/GJU2iIpF8s=")
                .SetVersion("1.14.0");

            manifest
                .DefineScript("bootstrap-select")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-select.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-select.js")
                //.SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta2/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta2/dist/js/bootstrap-select.js")
                //.SetCdnIntegrity("sha256-KK/CsQKh6Rb0LsRn4Z8Jcs4h7rRquelIb4EjQm6ige4=", "sha256-igJSxRuCIkTAOsoD48Fqd3mYbVGWL17ajzNLe+Ke2Hk=")
                .SetVersion("1.14.0");

            manifest
                .DefineStyle("nouislider")
                .SetUrl("~/OrchardCore.Resources/Styles/nouislider.min.css", "~/OrchardCore.Resources/Styles/nouislider.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.6.0/nouislider.min.css", "https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.6.0/nouislider.css")
                .SetCdnIntegrity("sha384-PSZaVsyG9jDu8hFaSJev5s/9poIJlX7cuxSGdqCgXRHpo2DzIaZAyCd2rG/DJJmV", "sha384-SW0/EWtnMakMnwC9RHA27DeNtNCLsJ0l+oZrXlFbb2123lhLdZIbiDiwRPogNY8T")
                .SetVersion("15.6.0");

            manifest
                .DefineScript("nouislider")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/nouislider/nouislider.min.js", "~/OrchardCore.Resources/Scripts/nouislider/nouislider.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.6.0/nouislider.min.js", "https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.6.0/nouislider.js")
                .SetCdnIntegrity("sha384-KiWE02DmrpKdHbQq8GswlJMAWML1i8bcHHKVrFiRIV6MFi8sLmFIbDMh4vQtX1Cd", "sha384-Iek2GBf0UVr6cs/ouPtsOrm0qzhXLwtQkWrRKIaIdW54zCWDsdD9jreTGmQTeUml")
                .SetVersion("15.6.0");

            manifest
                .DefineStyle("codemirror")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/codemirror.min.css", "~/OrchardCore.Resources/Styles/codemirror/codemirror.css")
                .SetCdn(codeMirrorUrl + "codemirror.min.css", codeMirrorUrl + "codemirror.css")
                .SetCdnIntegrity("sha384-zaeBlB/vwYsDRSlFajnDd7OydJ0cWk+c2OWybl3eSUf6hW2EbhlCsQPqKr3gkznT", "sha384-bsaAhvdduZPAwUb7RRLRvDgtEtOsggrgjkr/EjPO1i/vdoi+DmdLaG79UOt6M5hD")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn(codeMirrorUrl + "codemirror.min.js", codeMirrorUrl + "codemirror.js")
                .SetCdnIntegrity("sha384-O43V+gPNc/RWDQZB8gBhVUuuzV41Rgx2f4FFknBPb7TRAxFYjJzWe3459efQkAHg", "sha384-UreCKu5/CDyVzHr0N+pHzRt8beA9fzOzdi8F1GgLJDBFI1p4HN1hWh0Q5HMHBrAe")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css")
                .SetCdn(codeMirrorUrl + "addon/display/fullscreen.min.css", codeMirrorUrl + "addon/display/fullscreen.css")
                .SetCdnIntegrity("sha384-uuIczW2AGKADJpvg6YiNBJQWE7duDkgQDkndYEsbUGaLm8SPJZzlly6hcEo0aTlW", "sha384-+glu1jsbG+T5ocmkeMvIYh5w07IXKxmJZaCdkNbVfpEr3xi+M0gopFSR/oLKXxio")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.css")
                .SetCdn(codeMirrorUrl + "addon/hint/show-hint.min.css", codeMirrorUrl + "addon/hint/show-hint.css")
                .SetCdnIntegrity("sha384-qqTWkykzuDLx4yDYa7bVrwNwBHuqVvklDUMVaU4eezgNUEgGbP8Zv6i3u8OmtuWg", "sha384-ZZbLvEvLoXKrHo3Tkh7W8amMgoHFkDzWe8IAm1ZgxsG5y35H+fJCVMWwr0YBAEGA")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-selection-active-line")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.js")
                .SetCdn(codeMirrorUrl + "addon/selection/active-line.min.js", codeMirrorUrl + "addon/selection/active-line.js")
                .SetCdnIntegrity("sha384-hcxaXyAtJ30s2NeDu1OHWsQRiHiWuYLTbI596+YFb+f2pFhzO0mDuahZziRPPDxg", "sha384-kKz13r+qZMgTNgXROGNHQ0/0/J1FtvIvRZ9yjOHo1YLUCd+KF8r9R+su/B+f6C0U")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-display-autorefresh")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
                .SetCdn(codeMirrorUrl + "addon/display/autorefresh.min.js", codeMirrorUrl + "addon/display/autorefresh.js")
                .SetCdnIntegrity("sha384-pn83o6MtS8kicn/sV6AhRaBqXQ5tau8NzA2ovcobkcc1uRFP7D8CMhRx231QwKST", "sha384-5wrQkkCzj5dJInF+DDDYjE1itTGaIxO+TL6gMZ2eZBo9OyWLczkjolFX8kixX/9X")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn(codeMirrorUrl + "addon/display/fullscreen.min.js", codeMirrorUrl + "addon/display/fullscreen.js")
                .SetCdnIntegrity("sha384-mlEZFcWl5HzvZ6rIROEnNm825OC0Gw5KMZkilPtaJL7BGiluUu4c8Ws3IaNauZTh", "sha384-kuwHYEheDAp0NlPye2VXyxJ6J54CeVky9tH+e0mn6/gP4PJ50MF4FGKkrGAxIyCW")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-edit-closetag")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn(codeMirrorUrl + "addon/edit/closetag.min.js", codeMirrorUrl + "addon/edit/closetag.js")
                .SetCdnIntegrity("sha384-WIyvbwMte2q6VXgBkN7prVo9ZSmBm47nI2ftVjoJLPY4yOu9gmI6lqaNXjBwHU5k", "sha384-jR3Qxv7tHnv4TLLymC5s7Tl3aQGWNayqUHHBNxnnA/NjIyewLGSmNPQuDcMxnPKY")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.js")
                .SetCdn(codeMirrorUrl + "addon/hint/show-hint.min.js", codeMirrorUrl + "addon/hint/show-hint.js")
                .SetCdnIntegrity("sha384-RzmY798hMmzDwNL4d5ZkSaygnsA6Rq2ROY7awxW1t9NV+8XPpnXivbcD+EI1r2Ij", "sha384-RzmY798hMmzDwNL4d5ZkSaygnsA6Rq2ROY7awxW1t9NV+8XPpnXivbcD+EI1r2Ij")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-hint-sql-hint")
                .SetDependencies("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.js")
                .SetCdn(codeMirrorUrl + "addon/hint/sql-hint.min.js", codeMirrorUrl + "addon/hint/sql-hint.js")
                .SetCdnIntegrity("sha384-TT6FzU/qfXKsyGpknyPBSW9YUv9boLL9TStdfYRJTMjLDdUaQwWceBVs8I326z16", "sha384-CnIooEvOzoAsliTWO+BYwoTVFeqhBCympzDuwlhTlP7TWeAHB2h4Q0OeOqVkL5dP")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn(codeMirrorUrl + "addon/mode/multiplex.min.js", codeMirrorUrl + "addon/mode/multiplex.js")
                .SetCdnIntegrity("sha384-kZLDZbOp+G4qzGg8vy4P5DlIIPPnoSdO6G1puag9t/QjRDDfeANKKNIhMA+Yq+J1", "sha384-kZLDZbOp+G4qzGg8vy4P5DlIIPPnoSdO6G1puag9t/QjRDDfeANKKNIhMA+Yq+J1")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn(codeMirrorUrl + "addon/mode/simple.min.js", codeMirrorUrl + "addon/mode/simple.js")
                .SetCdnIntegrity("sha384-5+aYjV0V2W3IwhAYp/9WOrGMv1TaYkCjnkkW7Hv3yJQo28MergRCSRaUIUzDUs2J", "sha384-Foc3817k/CG8PVkPQtWgpBPcO0w2Noaep609FulvP/EwVUhv4JqNakD8UAORWA/s")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-css")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.js")
                .SetCdn(codeMirrorUrl + "mode/css/css.min.js", codeMirrorUrl + "mode/css/css.js")
                .SetCdnIntegrity("sha384-lD52V77ifAnw4prEYSP/ofB2xXosyjpS6kIFI9sniKdTFZFnPQvbzpLOredrEPNa", "sha384-buv/7jrLTn5IxOfA2PT/NP5GUeEgMVX2If4yqijcp+B5RRjJsRMvWqyQQEa3YRuJ")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-htmlmixed")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.js")
                .SetCdn(codeMirrorUrl + "mode/htmlmixed/htmlmixed.min.js", codeMirrorUrl + "mode/htmlmixed/htmlmixed.js")
                .SetCdnIntegrity("sha384-xYIbc5F55vPi7pb/lUnFj3wu24HlpAMZdtBHkNrb2YhPzJV3pX7+eqXT2PXSNMrw", "sha384-S/ZfY0a679JKW6L+0t1HLnsx5Vc/gShRnFGNzQ6ocpJ14AYeOXA1lFFf8mP9kUnM")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn(codeMirrorUrl + "mode/javascript/javascript.min.js", codeMirrorUrl + "mode/javascript/javascript.js")
                .SetCdnIntegrity("sha384-kmQrbJf09Uo1WRLMDVGoVG3nM6F48frIhcj7f3FDUjeRzsiHwyBWDjMUIttnIeAf", "sha384-dQontr0j5BEkZePsZgxyO763irzpMqG1Ns+fNyqzEdDoX+2m11rOOLCRVHDTaiCX")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn(codeMirrorUrl + "mode/sql/sql.min.js", codeMirrorUrl + "mode/sql/sql.js")
                .SetCdnIntegrity("sha384-pu0h4Jfq0nCLzBSLa5Vv4vg/WPB3GBU69Aqb5nghcmPLcIpappHGswX5kHeB+I2x", "sha384-1S1vSImC8LDHkRPqrwcYrRuwTItzy7NMUln1cN3Wz9Qo12UkGKka3BLV0/OP87mF")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn(codeMirrorUrl + "mode/xml/xml.min.js", codeMirrorUrl + "mode/xml/xml.js")
                .SetCdnIntegrity("sha384-xPpkMo5nDgD98fIcuRVYhxkZV6/9Y4L8s3p0J5c4MxgJkyKJ8BJr+xfRkq7kn6Tw", "sha384-fnrvkyvE4Ut04qDu6kFoulvy6uuBXr8YoD8rk13BCDTODAyMB5HTeBHKV+oq86Cl")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Styles/font-awesome.min.css", "~/OrchardCore.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.min.css", "~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.4/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.4/css/all.css")
                .SetCdnIntegrity("sha384-DyZ88mC6Up2uqS4h/KRgHuoeGwBcD4Ng9SiP4dIRy0EXTlnuz47vAwmeGwVChigm", "sha384-7rgjkhkxJ95zOzIjk97UrBOe14KgYpH9+zQm5BdgzjQELBU6kHf4WwoQzHfTx5sw")
                .SetVersion("5.15.4");

            manifest
                .DefineScript("font-awesome")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.4/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.4/js/all.js")
                .SetCdnIntegrity("sha384-rOA1PnstxnOBLzCLMcre8ybwbTmemjzdNlILg8O7z1lUkLXozs4DHonlDtnE7fpc", "sha384-HfU7cInvKb8zxQuLKtKr/suuRgcSH1OYsdJU+8lGA/t8nyNgdJF09UIkRzg1iefj")
                .SetVersion("5.15.4");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.4/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.4/js/v4-shims.js")
                .SetCdnIntegrity("sha384-bx00wqJq+zY9QLCMa/zViZPu1f0GJ3VXwF4GSw3GbfjwO28QCFr4qadCrNmJQ/9N", "sha384-SGuqaGE4bcW7Xl5T06BsUPUA91qaNtT53uGOcGpavQMje3goIFJbDsC0VAwtgL5g")
                .SetVersion("5.15.4");
            
            manifest
                .DefineStyle("font-awesome")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.1/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.1/css/all.css")
                .SetCdnIntegrity("sha384-/frq1SRXYH/bSyou/HUp/hib7RVN1TawQYja658FEOodR/FQBKVqT9Ol+Oz3Olq5", "sha384-8MuJL8Pm75smIw2IQ/ndnD1bU2d6byctZmsLNXTOzwY/3Eo6BLQpbd2vuohaDIGj")
                .SetVersion("6.1.1");

            manifest
                .DefineScript("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.1/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.1/js/all.js")
                .SetCdnIntegrity("sha384-xBXmu0dk1bEoiwd71wOonQLyH+VpgR1XcDH3rtxrLww5ajNTuMvBdL5SOiFZnNdp", "sha384-VPK9K2GnRj57/Sb0YKBihcOjPwUpnceZvWi2TGo0G068LyS0WEXa6kCOM1WVAVXJ")
                .SetVersion("6.1.1");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.1/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.1/js/v4-shims.js")
                .SetCdnIntegrity("sha384-lUDzd+x9AFWWpLYlM0ZCD+x586cN20gzVDrjHh8HUz22j1QwqTKQGkmd64bfBeZi", "sha384-Io52CmwR5olX5fLtbvu2wEUwQNoxsO/KhpweJ0MnfUljtk3QSLJPbXOoUPAdhIop")
                .SetVersion("6.1.1");

            manifest
                .DefineScript("jquery-resizable")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-resizable.min.js", "~/OrchardCore.Resources/Scripts/jquery-resizable.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/jquery-resizable-dom@0.35.0/dist/jquery-resizable.min.js")
                .SetCdnIntegrity("sha384-1LMjDEezsSgzlRgsyFIAvLW7FWSdFIHqBGjUa+ad5EqtK1FORC8XpTJ/pahxj5GB", "sha384-0yk9X0IG0cXxuN9yTTkps/3TNNI9ZcaKKhh8dgqOEAWGXxIYS5xaY2as6b32Ov3P")
                .SetVersion("0.35.0");

            manifest
                .DefineStyle("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg.min.css", "~/OrchardCore.Resources/Styles/trumbowyg.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.25.1/dist/ui/trumbowyg.min.css", "https://cdn.jsdelivr.net/npm/trumbowyg@2.25.1/dist/ui/trumbowyg.css")
                .SetCdnIntegrity("sha384-kTb2zOBw/Vng2V8SH/LF2llnbNnFpSMVTOrSN0W8Z0zzE8LMM+w/vFqhD9b+esLV", "sha384-qaIYw5IcvXvWBzXq2NPvCMucQerh0hReNg45Qas5X71KWzZmGuPp+VTW5zywbg0x")
                .SetVersion("2.25.1");

            manifest
                .DefineScript("trumbowyg")
                .SetDependencies("jquery-resizable")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.25.1/dist/trumbowyg.min.js", "https://cdn.jsdelivr.net/npm/trumbowyg@2.25.1/dist/trumbowyg.js")
                .SetCdnIntegrity("sha384-wwt6vSsdmnPNAuXp11Jjm37wAA+b/Rm33Jhd3QE/5E4oDLeNoCTLU3kxQYsdOtdW", "sha384-N2uP/HqSD9qptuEGY2J1Iq0G5jqaYUcuMGUnyWiS6giy4adYptw2Co8hT7LQJKcT")
                .SetVersion("2.25.1");

            manifest
                .DefineScript("trumbowyg-shortcodes")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg.shortcodes.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg.shortcodes.js")
                .SetVersion("1.0.0");

            manifest
                .DefineStyle("trumbowyg-plugins")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg-plugins.min.css", "~/OrchardCore.Resources/Styles/trumbowyg-plugins.css")
                .SetVersion("2.25.1");

            manifest
                .DefineScript("trumbowyg-plugins")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg-plugins.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg-plugins.js")
                .SetVersion("2.25.1");

            manifest
                .DefineScript("vuejs")
                .SetUrl("~/OrchardCore.Resources/Scripts/vue.min.js", "~/OrchardCore.Resources/Scripts/vue.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/vue@2.6.14/dist/vue.min.js", "https://cdn.jsdelivr.net/npm/vue@2.6.14/dist/vue.js")
                .SetCdnIntegrity("sha384-ULpZhk1pvhc/UK5ktA9kwb2guy9ovNSTyxPNHANnA35YjBQgdwI+AhLkixDvdlw4", "sha384-t1tHLsbM7bYMJCXlhr0//00jSs7ZhsAhxgm191xFsyzvieTMCbUWKMhFg9I6ci8q")
                .SetVersion("2.6.14");

            manifest
                .DefineScript("vue-multiselect")
                .SetDependencies("vuejs")
                .SetUrl("~/OrchardCore.Resources/Scripts/vue-multiselect.min.js", "~/OrchardCore.Resources/Scripts/vue-multiselect.min.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/vue-multiselect@2.1.6/dist/vue-multiselect.min.js", "https://cdn.jsdelivr.net/npm/vue-multiselect@2.1.6/dist/vue-multiselect.min.js")
                .SetCdnIntegrity("sha384-a4eXewRTYCwYdFtSnMCZTNtiXrfdul6aQdueRgHPAx2y1Ldp0QaFdCTpOx0ycsXU", "sha384-a4eXewRTYCwYdFtSnMCZTNtiXrfdul6aQdueRgHPAx2y1Ldp0QaFdCTpOx0ycsXU")
                .SetVersion("2.1.6");

            manifest
                .DefineStyle("vue-multiselect")
                .SetUrl("~/OrchardCore.Resources/Styles/vue-multiselect.min.css", "~/OrchardCore.Resources/Styles/vue-multiselect.min.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/vue-multiselect@2.1.6/dist/vue-multiselect.min.css", "https://cdn.jsdelivr.net/npm/vue-multiselect@2.1.6/dist/vue-multiselect.min.css")
                .SetCdnIntegrity("sha384-PPH/T7V86Z1+B4eMPef4FJXLD5fsTpObWoCoK3CiNtSX7aji+5qxpOCn1f2TDYAM", "sha384-PPH/T7V86Z1+B4eMPef4FJXLD5fsTpObWoCoK3CiNtSX7aji+5qxpOCn1f2TDYAM")
                .SetVersion("2.1.6");

            manifest
                .DefineScript("Sortable")
                .SetUrl("~/OrchardCore.Resources/Scripts/Sortable.min.js", "~/OrchardCore.Resources/Scripts/Sortable.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/sortablejs@1.14.0/Sortable.min.js", "https://cdn.jsdelivr.net/npm/sortablejs@1.14.0/Sortable.js")
                .SetCdnIntegrity("sha384-vxc713BCZYoMxC6DlBK6K4M+gLAS8+63q7TtgB2+KZVn8GNafLKZCJ7Wk2S6ZEl1", "sha384-6dbyp5R22OLsNh2CMeCLIc+s0ZPLpCtBG2f38vvi5ghQIfUvVHIzKpz2S3qLx83I")
                .SetVersion("1.14.0");

            manifest
                .DefineScript("vuedraggable")
                .SetDependencies("vuejs", "Sortable")
                .SetUrl("~/OrchardCore.Resources/Scripts/vuedraggable.umd.min.js", "~/OrchardCore.Resources/Scripts/vuedraggable.umd.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/vuedraggable@2.24.3/dist/vuedraggable.umd.min.js", "https://cdn.jsdelivr.net/npm/vuedraggable@2.24.3/dist/vuedraggable.umd.js")
                .SetCdnIntegrity("sha384-qUA1xXJiX23E4GOeW/XHtsBkV9MUcHLSjhi3FzO08mv8+W8bv5AQ1cwqLskycOTs", "sha384-+jB9vXc/EaIJTlNiZG2tv+TUpKm6GR9HCRZb3VkI3lscZWqrCYDbX2ZXffNJldL9")
                .SetVersion("2.24.3");

            manifest
                .DefineScript("js-cookie")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/js.cookie.min.js", "~/OrchardCore.Resources/Scripts/js.cookie.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/js-cookie@3.0.1/dist/js.cookie.min.js", "https://cdn.jsdelivr.net/npm/js-cookie@3.0.1/dist/js.cookie.js")
                .SetCdnIntegrity("sha384-ETDm/j6COkRSUfVFsGNM5WYE4WjyRgfDhy4Pf4Fsc8eNw/eYEMqYZWuxTzMX6FBa", "sha384-wAGdUDEOVO9JhMpvNh7mkYd0rL2EM0bLb1+VY5R+jDfVBxYFJNfzkinHfbRfxT2s")
                .SetVersion("3.0.1");

            manifest
                .DefineScript("monaco-loader")
                .SetUrl("~/OrchardCore.Resources/Scripts/monaco/vs/loader.js")
                .SetPosition(ResourcePosition.Last)
                .SetVersion(monacoEditorVersion);

            manifest
                .DefineScript("monaco")
                .SetAttribute("data-tenant-prefix", _pathBase)
                .SetUrl("~/OrchardCore.Resources/Scripts/monaco/ocmonaco.js")
                .SetDependencies("monaco-loader")
                .SetVersion(monacoEditorVersion);

            return manifest;
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(BuildManifest());

            var settings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            switch (settings.ResourceDebugMode)
            {
                case ResourceDebugMode.Enabled:
                    options.DebugMode = true;
                    break;

                case ResourceDebugMode.Disabled:
                    options.DebugMode = false;
                    break;

                case ResourceDebugMode.FromConfiguration:
                    options.DebugMode = !_env.IsProduction();
                    break;
            }

            options.UseCdn = settings.UseCdn;

            options.CdnBaseUrl = settings.CdnBaseUrl;

            options.AppendVersion = settings.AppendVersion;

            options.ContentBasePath = _httpContextAccessor.HttpContext.Request.PathBase.Value;
        }
    }
}
