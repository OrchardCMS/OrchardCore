using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private readonly ResourceOptions _resourceOptions;
        private readonly IHostEnvironment _env;
        private readonly PathString _pathBase;
        // Versions
        private const string codeMirrorVersion = "5.65.7";
        private const string monacoEditorVersion = "0.33.0";
        // URLs
        private const string cloudflareUrl = "https://cdnjs.cloudflare.com/ajax/libs/";
        private const string codeMirrorUrl = cloudflareUrl + "codemirror/" + codeMirrorVersion + "/";

        public ResourceManagementOptionsConfiguration(
            IOptions<ResourceOptions> resourceOptions,
            IHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _resourceOptions = resourceOptions.Value;
            _env = env;
            _pathBase = httpContextAccessor.HttpContext.Request.PathBase;
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
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta2/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta2/dist/js/bootstrap-select.js")
                .SetCdnIntegrity("sha256-KK/CsQKh6Rb0LsRn4Z8Jcs4h7rRquelIb4EjQm6ige4=", "sha256-igJSxRuCIkTAOsoD48Fqd3mYbVGWL17ajzNLe+Ke2Hk=")
                .SetVersion("1.14.0");

            manifest
                .DefineStyle("nouislider")
                .SetUrl("~/OrchardCore.Resources/Styles/nouislider.min.css", "~/OrchardCore.Resources/Styles/nouislider.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.6.1/nouislider.min.css", "https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.6.1/nouislider.css")
                .SetCdnIntegrity("sha384-PSZaVsyG9jDu8hFaSJev5s/9poIJlX7cuxSGdqCgXRHpo2DzIaZAyCd2rG/DJJmV", "sha384-SW0/EWtnMakMnwC9RHA27DeNtNCLsJ0l+oZrXlFbb2123lhLdZIbiDiwRPogNY8T")
                .SetVersion("15.6.1");

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
                .SetCdnIntegrity("sha512-uf06llspW44/LZpHzHT6qBOIVODjWtv4MxCricRxkzvopAlSWnTf6hpZTFxuuZcuNE9CBQhqE0Seu1CoRk84nQ==", "sha512-7vaQ4LLdaXd2IuMd4MUQ6LRFIGbEwJI1aq6KYqL3RjbdQyUkRFhwZKmqmkBXurTFdGlx687lTN8FSJfX6Df8Gw==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn(codeMirrorUrl + "codemirror.min.js", codeMirrorUrl + "codemirror.js")
                .SetCdnIntegrity("sha512-3S64QagKiTlNjSfuh3UYtYSkP494WHoWc96YvbmB2BReHpNtxlrMNY6MbJLDpavcgD8Pj5p44F/PY586uVO5iA==", "sha512-4qmqmrlj6og5ngz20GlGWrzGdquJl/CsYbCzWVl5Jl7Zwztb2t2Ys8z/HrylnAsPO50HkyZDYUq+1xbrHtpn6g==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-display-autorefresh")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
                .SetCdn(codeMirrorUrl + "addon/display/autorefresh.min.js", codeMirrorUrl + "addon/display/autorefresh.js")
                .SetCdnIntegrity("sha512-vAsKB7xXQAWMn5kcwda0HkFVKUxSYwrmrGprVhmbGFNAG1Ij+2epT3zzdwjHTJyDsKXsiEdrUdhIxh7loHyX+A==", "sha512-sy92U6wPlP/bYMqxET3Cg3EsXO588d27OsdUH/X1JnkEvjDgUeDEBbh4Ak0Si6YP7hZwwNkXBdu6iJJANWUD5w==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css")
                .SetCdn(codeMirrorUrl + "addon/display/fullscreen.min.css", codeMirrorUrl + "addon/display/fullscreen.css")
                .SetCdnIntegrity("sha512-T8xB3MmwpA77VK9lUH3UkdUTnkmpqOxHF8OceOKaHrvpcXMSNX0xtpa9FoLTDAVO1JnB2UiMdVeI2V0HTHjTWA==", "sha512-cCwzc1kXKEd8Z9s559PoYPH1HMjHeQvCAiE0KaWQZ7A3XbC0ZjtTP91bZFeGZLDvA7um51WCnRcK2XLHWyODTQ==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn(codeMirrorUrl + "addon/display/fullscreen.min.js", codeMirrorUrl + "addon/display/fullscreen.js")
                .SetCdnIntegrity("sha512-8Rk4wB3MfgQfuOw95XW/vAU8PSTHWI5HddVRXfg+Ykk8CzsiOZkDpxVSrnYq2u/IqO499ooZ61fFHppuzB2ghA==", "sha512-3LSEeg+b0pIeBxYCn3cWoBoitqDAlMImCNQp2D1Ea7LaZ8DT94m9hIJ7fjuSzBFFUswsZ+hp+X0+6KCpHKv/Ag==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-edit-closetag")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.js")
                .SetCdn(codeMirrorUrl + "addon/edit/closetag.min.js", codeMirrorUrl + "addon/edit/closetag.js")
                .SetCdnIntegrity("sha512-XYx5xhl4B5vKNlaRBWh/nlti0+IPM6eT+dSFc3/oc4rERn2DpwbS3q4OblprqqBLXyRSVePKmf+8mHkDLtGZpg==", "sha512-1aYjqb0gMQmBzv97uzDI+zeMkiJ8cBIszGw4/k48sHZ/lzdq1Ibu/XxRGCMtv5TDwbtSddTs1IP/2zKKUaiRZw==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.css")
                .SetCdn(codeMirrorUrl + "addon/hint/show-hint.min.css", codeMirrorUrl + "addon/hint/show-hint.css")
                .SetCdnIntegrity("sha512-OmcLQEy8iGiD7PSm85s06dnR7G7C9C0VqahIPAj/KHk5RpOCmnC6R2ob1oK4/uwYhWa9BF1GC6tzxsC8TIx7Jg==", "sha512-8MR8AFnfQwZW7Ta4AhsbUdyipTivUNe+gFULBYDX6u7k7ER4R9K50kcOd88lq3OCUBKrkAW3jJ1+ooxhagkytw==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.js")
                .SetCdn(codeMirrorUrl + "addon/hint/show-hint.min.js", codeMirrorUrl + "addon/hint/show-hint.js")
                .SetCdnIntegrity("sha512-yhmeAerubMLaGAsyS7sE8Oqub6GeTkBDQpkXo2JKHgg7JOCudQvcbDQc5rPxdl7MqcDusTJzSy+ODlyzAwETfQ==", "sha512-pjhQkESWQCP08lFbib1SNiOWyirUf6r5eNvAxTK4Ra3bVz3omuZW5+5kaFYNJy8R3DortMYLMXWwl0FegS3pKQ==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-hint-sql-hint")
                .SetDependencies("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.js")
                .SetCdn(codeMirrorUrl + "addon/hint/sql-hint.min.js", codeMirrorUrl + "addon/hint/sql-hint.js")
                .SetCdnIntegrity("sha512-O7YCIZwiyJYc9d/iPOSgEzhhlonTMGcmM1HmgYFffj5cGwVu2PLSzTaLvD9HSk8rSSf9rIpdhJPk8Yhu6wJBtA==", "sha512-i6qVmr4jXdJZFuABds8gHtQ7fKTRUubUxdSGApUOdL2B12xRmaAvc5LocgXaf61l5QHJs6NynpIvQQxu5qQ1JA==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn(codeMirrorUrl + "addon/mode/multiplex.min.js", codeMirrorUrl + "addon/mode/multiplex.js")
                .SetCdnIntegrity("sha512-SnKl5kuiZJ6HI+aRXBjW1qv8USObXnN8BoPLHLQ7igatyz0QhbFgNYba9g8+zPsaYXzOm1XIrsj/b1uazysMtA==", "sha512-xGdyL/q8XtWD05NETb/o7ONYii6T4zGvjUL22FvgfeM3pUb7EvyIFSsLBQzyxb2hCYdH7DKNXqw99Hcf9Onkiw==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn(codeMirrorUrl + "addon/mode/simple.min.js", codeMirrorUrl + "addon/mode/simple.js")
                .SetCdnIntegrity("sha512-CGM6DWPHs250F/m90YZ9NEiEUhd9a4+u8wAzeKC6uHzZbYyt9/e2dLC5BGGB6Y0HtEdZQdSDYjDsoTyNGdMrMA==", "sha512-XVXmeKXwCPr9QCgdbZ6cyCU3oFMxapqhHBXHhK12Qj4x60NgwKdOLAU/khSaIqd8XMO9Ll+sUk68qHsGVKV0Dg==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-selection-active-line")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.js")
                .SetCdn(codeMirrorUrl + "addon/selection/active-line.min.js", codeMirrorUrl + "addon/selection/active-line.js")
                .SetCdnIntegrity("sha512-0sDhEPgX5DsfNcL5ty4kP6tR8H2vPkn40GwA0RYTshkbksURAlsRVnG4ECPPBQh7ZYU6S3rGvp5uhlGQUNrcmA==", "sha512-DvG6G3Tewtulx8KxRQNl8ozWU6Wic4LSeSazrouQqctgYLeuhfabduYK4ihtHajhn16+HSNJ9tj9J7xazBzcZw==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-css")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.js")
                .SetCdn(codeMirrorUrl + "mode/css/css.min.js", codeMirrorUrl + "mode/css/css.js")
                .SetCdnIntegrity("sha512-rQImvJlBa8MV1Tl1SXR5zD2bWfmgCEIzTieFegGg89AAt7j/NBEe50M5CqYQJnRwtkjKMmuYgHBqtD1Ubbk5ww==", "sha512-kbf0gMc1+KPTTDWkjkGwf3h2Nx4djnWBVtcCKJcwLQpQ/TIMAjRBKM9f0ZDgj9kT5WTs1Qzsq+5Si2p7j9whkg==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-htmlmixed")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.js")
                .SetCdn(codeMirrorUrl + "mode/htmlmixed/htmlmixed.min.js", codeMirrorUrl + "mode/htmlmixed/htmlmixed.js")
                .SetCdnIntegrity("sha512-HN6cn6mIWeFJFwRN9yetDAMSh+AK9myHF1X9GlSlKmThaat65342Yw8wL7ITuaJnPioG0SYG09gy0qd5+s777w==", "sha512-ooMFWcVphTl+L7BsQk/PRDyUyFYH7f2QHjJ0kFvkTxJgMJS8XXPDJBZIrBtM0P7TCDOPjfQcnbx3s8hp27vraw==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn(codeMirrorUrl + "mode/javascript/javascript.min.js", codeMirrorUrl + "mode/javascript/javascript.js")
                .SetCdnIntegrity("sha512-I6CdJdruzGtvDyvdO4YsiAq+pkWf2efgd1ZUSK2FnM/u2VuRASPC7GowWQrWyjxCZn6CT89s3ddGI+be0Ak9Fg==", "sha512-9bZKsY+97AQthWWS54sUIWxo0GkS0yPvSXStcMshHDVj9lCfLgzhSTr9xa+Ho0WKkRTKMm2yy8M+EG10n9u0iQ==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn(codeMirrorUrl + "mode/sql/sql.min.js", codeMirrorUrl + "mode/sql/sql.js")
                .SetCdnIntegrity("sha512-JOURLWZEM9blfKvYn1pKWvUZJeFwrkn77cQLJOS6M/7MVIRdPacZGNm2ij5xtDV/fpuhorOswIiJF3x/woe5fw==", "sha512-1wJ2qS5s2fgMsHXC7tbhkE0UyMZyoHAPuTtPIv+SfJW+JTPF5pf4wqbB1hRs4QCFTYoGwZ6n0nxn6PZtqfpk/g==")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn(codeMirrorUrl + "mode/xml/xml.min.js", codeMirrorUrl + "mode/xml/xml.js")
                .SetCdnIntegrity("sha512-LarNmzVokUmcA7aUDtqZ6oTS+YXmUKzpGdm8DxC46A6AHu+PQiYCUlwEGWidjVYMo/QXZMFMIadZtrkfApYp/g==", "sha512-zWiFnvx+BKIYp7HwMXQRKR3SC8AiS6wQchjjjtAV9bt34F54fwJMB//7hVm0C+kQywyStbJ00u9i07Qw9ARuqQ==")
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
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.2/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.2/css/all.css")
                .SetCdnIntegrity("sha384-fZCoUih8XsaUZnNDOiLqnby1tMJ0sE7oBbNk2Xxf5x8Z4SvNQ9j83vFMa/erbVrV", "sha384-IUVDBMpdggHqukUHtkKHQXVpgREwTTkAI1HIiKclMQDiBwixzX+TvzTrF/NXiQGd")
                .SetVersion("6.1.2");

            manifest
                .DefineScript("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.2/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.2/js/all.js")
                .SetCdnIntegrity("sha384-11X1bEJVFeFtn94r1jlvSC7tlJkV2VJctorjswdLzqOJ6ZvYBSZQkaQVXG0R4Flt", "sha384-5a4z4ROkRprwLJonyifydHsCtOAFcmvE0v7ujAyc6jlNCJIHvD8DlKGf/UcHO4Ud")
                .SetVersion("6.1.2");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.2/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.1.2/js/v4-shims.js")
                .SetCdnIntegrity("sha384-JwJ3z2CNw6j4LN4k+tA6GEN2OQSUzcSBpWIsEqlngCZqnfxDsQUe5SURjhpXLhvY", "sha384-qcECdQX7GRYtMQ569Y26DGnAm4FTUGVNCcWVL8XlzHQk5+dNCuUsqYxGDvGbJmUe")
                .SetVersion("6.1.2");

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
                .SetCdn("https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js", "https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.js")
                .SetCdnIntegrity("sha384-eeLEhtwdMwD3X9y+8P3Cn7Idl/M+w8H4uZqkgD/2eJVkWIN1yKzEj6XegJ9dL3q0", "sha384-OFIl93h6jYoAF+hATXncsLYMiLo81FpuReH3fgCI4wep7qNCmiA2I0bwcvVqHSBj")
                .SetVersion("1.15.0");

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

            switch (_resourceOptions.ResourceDebugMode)
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

            options.UseCdn = _resourceOptions.UseCdn;
            options.CdnBaseUrl = _resourceOptions.CdnBaseUrl;
            options.AppendVersion = _resourceOptions.AppendVersion;
            options.ContentBasePath = _pathBase.Value;
        }
    }
}
