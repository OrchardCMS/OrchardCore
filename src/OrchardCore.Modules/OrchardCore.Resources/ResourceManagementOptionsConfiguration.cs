using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private readonly ResourceOptions _resourceOptions;
    private readonly IHostEnvironment _env;
    private readonly PathString _pathBase;
    // Versions
    private const string CodeMirrorVersion = "5.65.7";
    private const string MonacoEditorVersion = "0.46.0";
    // URLs
    private const string CloudflareUrl = "https://cdnjs.cloudflare.com/ajax/libs/";
    private const string CodeMirrorUrl = CloudflareUrl + "codemirror/" + CodeMirrorVersion + "/";

    public ResourceManagementOptionsConfiguration(
        IOptions<ResourceOptions> resourceOptions,
        IHostEnvironment env,
        IHttpContextAccessor httpContextAccessor)
    {
        _resourceOptions = resourceOptions.Value;
        _env = env;
        _pathBase = httpContextAccessor.HttpContext.Request.PathBase;
    }

    private ResourceManifest BuildManifest()
    {
        var manifest = new ResourceManifest();

        manifest
            .DefineScript("jQuery")
            .SetUrl("~/OrchardCore.Resources/Scripts/jquery.min.js", "~/OrchardCore.Resources/Scripts/jquery.js")
            .SetCdn("https://code.jquery.com/jquery-3.7.1.min.js", "https://code.jquery.com/jquery-3.7.1.js")
            .SetCdnIntegrity("sha384-1H217gwSVyLSIfaLxHbE7dRb3v4mYCKbpQvzx0cegeju1MVsGrX5xXxAvs/HgeFs", "sha384-wsqsSADZR1YRBEZ4/kKHNSmU+aX8ojbnKUMN4RyD3jDkxw5mHtoe2z/T/n4l56U/")
            .SetVersion("3.7.1");

        manifest
            .DefineScript("jQuery.slim")
            .SetUrl("~/OrchardCore.Resources/Scripts/jquery.slim.min.js", "~/OrchardCore.Resources/Scripts/jquery.slim.js")
            .SetCdn("https://code.jquery.com/jquery-3.7.1.slim.min.js", "https://code.jquery.com/jquery-3.7.1.slim.js")
            .SetCdnIntegrity("sha384-5AkRS45j4ukf+JbWAfHL8P4onPA9p0KwwP7pUdjSQA3ss9edbJUJc/XcYAiheSSz", "sha384-5yyt26go0PtGiMk9qStZt+lySzAg8ZSY0i7q6l05kHEEChYiHvf0NsjlexoEdASI")
            .SetVersion("3.7.1");

        manifest
            .DefineScript("jQuery")
            .SetUrl("~/OrchardCore.Resources/Vendor/jquery-3.6.0/jquery.min.js", "~/OrchardCore.Resources/Scripts/jquery.js")
            .SetCdn("https://code.jquery.com/jquery-3.6.0.min.js", "https://code.jquery.com/jquery-3.6.0.js")
            .SetCdnIntegrity("sha384-vtXRMe3mGCbOeY7l30aIg8H9p3GdeSe4IFlP6G8JMa7o7lXvnz3GFKzPxzJdPfGK", "sha384-S58meLBGKxIiQmJ/pJ8ilvFUcGcqgla+mWH9EEKGm6i6rKxSTA2kpXJQJ8n7XK4w")
            .SetVersion("3.6.0");

        manifest
            .DefineScript("jQuery.slim")
            .SetUrl("~/OrchardCore.Resources/Vendor/jquery-3.6.0/jquery.slim.min.js", "~/OrchardCore.Resources/Scripts/jquery.slim.js")
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
            .SetCdnIntegrity("sha384-VHvPCCyXqtD5DqJeNxl2dtTyhF78xXNXdkwX1CZeRusQfRKp+tA7hAShOK/B/fQ2", "sha384-acUSOMj/FOTIzZ4qpiZXd/6avQezsTkra+wBPeduOyUIA5anC5YcLndJ3Wn4b4pF")
            .SetVersion("4.6.1");

        manifest
            .DefineScript("bootstrap-bundle")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.bundle.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.bundle.js")
            .SetCdnIntegrity("sha384-fQybjgWLrvvRgtW6bFlB7jaZrFsaBXjsOMm/tB9LTS58ONXgqbR9W8oWht/amnpF", "sha384-fAPB/5gOv3oOdZZ9/se34OICi8gkYHlNY5skCVFZPzGe+F2puEgM5hu8ctpXyKFM")
            .SetVersion("4.6.1");

        manifest
            .DefineStyle("bootstrap")
            .SetUrl("~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.min.css", "~/OrchardCore.Resources/Vendor/bootstrap-4.6.1/bootstrap.css")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.css")
            .SetCdnIntegrity("sha384-zCbKRCUGaJDkqS1kPbPd7TveP5iyJE0EjAuZQTgFLD2ylzuqKfdKlfG/eSrtxUkn", "sha384-ztSeENTvhymkwcI8wMyrHLHIyPJgek5ErHOMw9p96EzJKwbiuJBWBDuPJpGNqOar")
            .SetVersion("4.6.1");

        manifest
           .DefineScript("popperjs")
           .SetUrl("~/OrchardCore.Resources/Scripts/popper.min.js", "~/OrchardCore.Resources/Scripts/popper.js")
           .SetCdn("https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.8/dist/umd/popper.min.js", "https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.8/dist/umd/popper.js")
           .SetCdnIntegrity("sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r", "sha384-yBknSWNrSUPkBtbhhCJ07i/BOmbrigRhLKPzTAny+TT4uGAwIdfNTAkBd/3VzXbg")
           .SetVersion("2.11.8");

        manifest
            .DefineScript("bootstrap")
            .SetDependencies("popperjs")
            .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.js")
            .SetCdnIntegrity("sha384-0pUGZvbkm6XF6gxjEnlmuGrJXVbNuzT9qBBavbLwCsOGabYfZo0T0to5eqruptLy", "sha384-jYoAmFjufJZfBzwTARyz2gk7Jj9mQb2cLeP9n5PcgLCVVd+8QfjY1+qFj+rBkViV")
            .SetVersion("5.3.3");

        manifest
            .DefineScript("bootstrap-bundle")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.bundle.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.js")
            .SetCdnIntegrity("sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz", "sha384-5xO2n1cyGKAe630nacBqFQxWoXjUIkhoc/FxQrWM07EIZ3TuqkAsusDeyPDOIeid")
            .SetVersion("5.3.3");

        manifest
            .DefineStyle("bootstrap")
            .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.min.css", "~/OrchardCore.Resources/Styles/bootstrap.css")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.css")
            .SetCdnIntegrity("sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH", "sha384-qAlWxD5RDF+aEdUc1Z7GR/tE4zYjX1Igo/LrIexlnzM6G63a6F1fXZWpZKSrSW86")
            .SetVersion("5.3.3");

        manifest
            .DefineStyle("bootstrap-rtl")
            .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.rtl.min.css", "~/OrchardCore.Resources/Styles/bootstrap.rtl.css")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.css")
            .SetCdnIntegrity("sha384-nU14brUcp6StFntEOOEBvcJm4huWjB0OcIeQ3fltAfSmuZFrkAif0T+UtNGlKKQv", "sha384-CEku08bnqQAT/vzi6/zxMQmSyxoOTK1jx7mbT8P7etf/YhPbxASCX5BIVuAK9sfy")
            .SetVersion("5.3.3");

        manifest
            .DefineStyle("bootstrap-select")
            .SetUrl("~/OrchardCore.Resources/Styles/bootstrap-select.min.css", "~/OrchardCore.Resources/Styles/bootstrap-select.css")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta3/dist/css/bootstrap-select.min.css", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta3/dist/css/bootstrap-select.css")
            .SetCdnIntegrity("sha384-xF1Y2i6HgC34+4EWddbDhlQuru7cLSKRcPT3hoL3mPoKoV+624vVSZJmegPX77vS", "sha384-DtuOZ7LbR+xAYzDGD4YLpe9eiAayUBwZRqAcoy+RepIoV53tAoJbXnr4AX1xTJ43")
            .SetVersion("1.14.0");

        manifest
            .DefineScript("bootstrap-select")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-select.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-select.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta3/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta3/dist/js/bootstrap-select.js")
            .SetCdnIntegrity("sha384-0O3sg2SQIGn4393xwamQISjphC8DIXjCzlhj1gPAMC5xGg+2perF5Mehr5njv0fZ", "sha384-2b0aLFg/Ejp4OF57nW0BUqNzm259RHYYMf/mpKClBijsEH2P+4ea2oWAq0twd8L0")
            .SetVersion("1.14.0");

        manifest
            .DefineStyle("nouislider")
            .SetUrl("~/OrchardCore.Resources/Styles/nouislider.min.css", "~/OrchardCore.Resources/Styles/nouislider.css")
            .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.7.0/nouislider.min.css", "https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.7.0/nouislider.css")
            .SetCdnIntegrity("sha384-PSZaVsyG9jDu8hFaSJev5s/9poIJlX7cuxSGdqCgXRHpo2DzIaZAyCd2rG/DJJmV", "sha384-SW0/EWtnMakMnwC9RHA27DeNtNCLsJ0l+oZrXlFbb2123lhLdZIbiDiwRPogNY8T")
            .SetVersion("15.7.0");

        manifest
            .DefineScript("nouislider")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Resources/Scripts/nouislider/nouislider.min.js", "~/OrchardCore.Resources/Scripts/nouislider/nouislider.js")
            .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.7.0/nouislider.min.js", "https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.7.0/nouislider.js")
            .SetCdnIntegrity("sha384-/gBUOLHADjY2rp6bHB0IyW9AC28q4OsnirJScje4l1crgYW7Qarx3dH8zcqcUgmy", "sha384-ZRTsSqAkR2D5UR6P8ew9nDImNmAueqBx3QIljDVMucOjF3eVskkMIk50HUW239mY")
            .SetVersion("15.7.0");

        manifest
            .DefineStyle("codemirror")
            .SetUrl("~/OrchardCore.Resources/Styles/codemirror/codemirror.min.css", "~/OrchardCore.Resources/Styles/codemirror/codemirror.css")
            .SetCdn(CodeMirrorUrl + "codemirror.min.css", CodeMirrorUrl + "codemirror.css")
            .SetCdnIntegrity("sha384-zaeBlB/vwYsDRSlFajnDd7OydJ0cWk+c2OWybl3eSUf6hW2EbhlCsQPqKr3gkznT", "sha384-bsaAhvdduZPAwUb7RRLRvDgtEtOsggrgjkr/EjPO1i/vdoi+DmdLaG79UOt6M5hD")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
            .SetCdn(CodeMirrorUrl + "codemirror.min.js", CodeMirrorUrl + "codemirror.js")
            .SetCdnIntegrity("sha384-FIV1f0SplvlwhOxrMYdXpUcnVM79A9u4ffl0lEOVVZunLMuGpxFyQmnBHl24EQFR", "sha384-VAGlhrJE9eY6OkSfl3ioNR4jWUGp1mLeF/ECkHxBUEfTB9u2xVVH14acAUBDc3Y8")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-display-autorefresh")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
            .SetCdn(CodeMirrorUrl + "addon/display/autorefresh.min.js", CodeMirrorUrl + "addon/display/autorefresh.js")
            .SetCdnIntegrity("sha384-pn83o6MtS8kicn/sV6AhRaBqXQ5tau8NzA2ovcobkcc1uRFP7D8CMhRx231QwKST", "sha384-B1M1WS08oqd1y3zKAPdhkOSNwy+NYMREyK9qXNWl+QfXDlqj+Y+TYuBUkc/uAnox")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineStyle("codemirror-addon-display-fullscreen")
            .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css")
            .SetCdn(CodeMirrorUrl + "addon/display/fullscreen.min.css", CodeMirrorUrl + "addon/display/fullscreen.css")
            .SetCdnIntegrity("sha384-uuIczW2AGKADJpvg6YiNBJQWE7duDkgQDkndYEsbUGaLm8SPJZzlly6hcEo0aTlW", "sha384-+glu1jsbG+T5ocmkeMvIYh5w07IXKxmJZaCdkNbVfpEr3xi+M0gopFSR/oLKXxio")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-display-fullscreen")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
            .SetCdn(CodeMirrorUrl + "addon/display/fullscreen.min.js", CodeMirrorUrl + "addon/display/fullscreen.js")
            .SetCdnIntegrity("sha384-mlEZFcWl5HzvZ6rIROEnNm825OC0Gw5KMZkilPtaJL7BGiluUu4c8Ws3IaNauZTh", "sha384-vU/yRPnV0VIhELETYT5fG/k7uMzeHddzkMo4NFrLzHdJepeb46v0b61P8FADXtN4")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-edit-closetag")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.js")
            .SetCdn(CodeMirrorUrl + "addon/edit/closetag.min.js", CodeMirrorUrl + "addon/edit/closetag.js")
            .SetCdnIntegrity("sha384-WIyvbwMte2q6VXgBkN7prVo9ZSmBm47nI2ftVjoJLPY4yOu9gmI6lqaNXjBwHU5k", "sha384-i4ai3UXE5wIk3ILN77PB9DhNmku+sefNKDTHXRvsrYX2bxWzm+EDmoBui5wsNU2v")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineStyle("codemirror-addon-hint-show-hint")
            .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.css")
            .SetCdn(CodeMirrorUrl + "addon/hint/show-hint.min.css", CodeMirrorUrl + "addon/hint/show-hint.css")
            .SetCdnIntegrity("sha384-qqTWkykzuDLx4yDYa7bVrwNwBHuqVvklDUMVaU4eezgNUEgGbP8Zv6i3u8OmtuWg", "sha384-ZZbLvEvLoXKrHo3Tkh7W8amMgoHFkDzWe8IAm1ZgxsG5y35H+fJCVMWwr0YBAEGA")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-hint-show-hint")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.js")
            .SetCdn(CodeMirrorUrl + "addon/hint/show-hint.min.js", CodeMirrorUrl + "addon/hint/show-hint.js")
            .SetCdnIntegrity("sha384-8i/ZCyq/QakicQDFEJhSl5oJEzzChdhEHhFBvtlxJMWdzohX6j+7O7L7NGgVjaL2", "sha384-X99dMoDW1QAOqOa49NoR3mVQPZMAmWdk1EtGVfWHdqFrD0mKdc4TVCugISfW0wEv")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-hint-sql-hint")
            .SetDependencies("codemirror-addon-hint-show-hint")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.js")
            .SetCdn(CodeMirrorUrl + "addon/hint/sql-hint.min.js", CodeMirrorUrl + "addon/hint/sql-hint.js")
            .SetCdnIntegrity("sha384-TT6FzU/qfXKsyGpknyPBSW9YUv9boLL9TStdfYRJTMjLDdUaQwWceBVs8I326z16", "sha384-v0PZIWVaXK+SdJWDH/8f3lMh+4SXRujsh67aPj27BlUq4ocyn0Yime8qyi8AArtz")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-mode-multiplex")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
            .SetCdn(CodeMirrorUrl + "addon/mode/multiplex.min.js", CodeMirrorUrl + "addon/mode/multiplex.js")
            .SetCdnIntegrity("sha384-Qr/1hjoJmzEf6ToQLo1nr8/GF5ekpaqUx0DM71QHZ4N8+c8l8aqlQ25whgApmChE", "sha384-pgj6NfGJNeIBDFqYM8m/ah3J269WhaJDiT0fN/C0c335qDMJAbfz/O2rVB53w+1D")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-mode-simple")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
            .SetCdn(CodeMirrorUrl + "addon/mode/simple.min.js", CodeMirrorUrl + "addon/mode/simple.js")
            .SetCdnIntegrity("sha384-5+aYjV0V2W3IwhAYp/9WOrGMv1TaYkCjnkkW7Hv3yJQo28MergRCSRaUIUzDUs2J", "sha384-qeu+SDWpTAqXUyXdqdwbMDeYSQiv6rErEHLJ/cITY3wtuYhN2LdBSBZeS7Jyn8nv")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-addon-selection-active-line")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.js")
            .SetCdn(CodeMirrorUrl + "addon/selection/active-line.min.js", CodeMirrorUrl + "addon/selection/active-line.js")
            .SetCdnIntegrity("sha384-hcxaXyAtJ30s2NeDu1OHWsQRiHiWuYLTbI596+YFb+f2pFhzO0mDuahZziRPPDxg", "sha384-QhD10EHRrst6CIOzeEBXQhUT95YVJN1EV8uX2Jb5S3+qw/ozbvUE5Zn5uILnZg96")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-mode-css")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.js")
            .SetCdn(CodeMirrorUrl + "mode/css/css.min.js", CodeMirrorUrl + "mode/css/css.js")
            .SetCdnIntegrity("sha384-fpeIC2FZuPmw7mIsTvgB5BNc8QVxQC/nWg2W+CgPYOAiBiYVuHe2E8HiTWHBMIJQ", "sha384-ZD4C1ohrucZOfP7+jQSuBELICO7Z73CFD5stbjic1D3DbZk88mqj3KsRjSml/NCK")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-mode-htmlmixed")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.js")
            .SetCdn(CodeMirrorUrl + "mode/htmlmixed/htmlmixed.min.js", CodeMirrorUrl + "mode/htmlmixed/htmlmixed.js")
            .SetCdnIntegrity("sha384-xYIbc5F55vPi7pb/lUnFj3wu24HlpAMZdtBHkNrb2YhPzJV3pX7+eqXT2PXSNMrw", "sha384-0MH/N0DfWPIbCXDe9I7tmLw0dsJ4gKQUijwCcpaTgGxrTTrypsJwEOOqi5yhkQiK")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-mode-javascript")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
            .SetCdn(CodeMirrorUrl + "mode/javascript/javascript.min.js", CodeMirrorUrl + "mode/javascript/javascript.js")
            .SetCdnIntegrity("sha384-kmQrbJf09Uo1WRLMDVGoVG3nM6F48frIhcj7f3FDUjeRzsiHwyBWDjMUIttnIeAf", "sha384-fgstVTG7RpZvquT2ZtQeU9iJXfvK9wTstlhasVKML77E5oe3Wi0AfLzY2grsjwmh")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-mode-sql")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
            .SetCdn(CodeMirrorUrl + "mode/sql/sql.min.js", CodeMirrorUrl + "mode/sql/sql.js")
            .SetCdnIntegrity("sha384-cof/65v3Fn+7YeqmMp8aCI1xZ2yeX8JnWhQYdMzaO7VDpBOtLjfdwx6pEfUo7SFT", "sha384-Llr4/OHf89ufsfG7HbSwqB7KTd1d4hXok/Yxi72qWvLwtd6pz6nqJdOGlDdpjw6I")
            .SetVersion(CodeMirrorVersion);

        manifest
            .DefineScript("codemirror-mode-xml")
            .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
            .SetCdn(CodeMirrorUrl + "mode/xml/xml.min.js", CodeMirrorUrl + "mode/xml/xml.js")
            .SetCdnIntegrity("sha384-xPpkMo5nDgD98fIcuRVYhxkZV6/9Y4L8s3p0J5c4MxgJkyKJ8BJr+xfRkq7kn6Tw", "sha384-KX/+LdYWr3JcKfT7HK55DC3oPVJwnJSympb1qoO14sxVDtDIg+xHPVLltqJEbitI")
            .SetVersion(CodeMirrorVersion);

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
            .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.min.css", "~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.css")
            .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.6.0/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.6.0/css/all.css")
            .SetCdnIntegrity("sha384-h/hnnw1Bi4nbpD6kE7nYfCXzovi622sY5WBxww8ARKwpdLj5kUWjRuyiXaD1U2JT", "sha384-vMdawx0r3BjxHQwcfWi0YSemtW6u5mKxTKPPh1ogUICPLaEa/6e42yg2wRYzzJtx")
            .SetVersion("6.6.0");

        manifest
            .DefineScript("font-awesome")
            .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.6.0/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.6.0/js/all.js")
            .SetCdnIntegrity("sha384-dgEl3vRKux81M373f/TdgoDTV5oZj+yjHrr/1qR5b4btG5q63kYS62t5kod+7Q6v", "sha384-v72QeVpDL6Ne4X2S+fwIXCOLhO57lwIR/qRV05SHAExDg1QoyJGWDxx6VQO3rzlC")
            .SetVersion("6.6.0");

        manifest
            .DefineScript("font-awesome-v4-shims")
            .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.6.0/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.6.0/js/v4-shims.js")
            .SetCdnIntegrity("sha384-M9y++reQwf5nddw5loUHChCbGE4kwaeHzeEM2yWidMfaRMQeHM6MSwwPuiSnSMHF", "sha384-WrIndr6nwB5l/sVUyZkWDlpNS9Vx/Y+zhmjZNP+j2UJOBmy36ufaIIqd7o2kL0BL")
            .SetVersion("6.6.0");

        manifest
            .DefineScript("jquery-resizable")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Resources/Scripts/jquery-resizable.min.js", "~/OrchardCore.Resources/Scripts/jquery-resizable.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/jquery-resizable-dom@0.35.0/dist/jquery-resizable.min.js")
            .SetCdnIntegrity("sha384-1LMjDEezsSgzlRgsyFIAvLW7FWSdFIHqBGjUa+ad5EqtK1FORC8XpTJ/pahxj5GB", "sha384-0yk9X0IG0cXxuN9yTTkps/3TNNI9ZcaKKhh8dgqOEAWGXxIYS5xaY2as6b32Ov3P")
            .SetVersion("0.35.0");

        manifest
            .DefineStyle("trumbowyg")
            .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg/trumbowyg.min.css", "~/OrchardCore.Resources/Styles/trumbowyg/trumbowyg.css")
            .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.28.0/dist/ui/trumbowyg.min.css", "https://cdn.jsdelivr.net/npm/trumbowyg@2.28.0/dist/ui/trumbowyg.css")
            .SetCdnIntegrity("sha384-XfI6P0jtm0X3QDEQxS1DotzhIXkJeuSV1wtBOntPaRxzpzTkMWkpZpkuzj8qVHzl", "sha384-GUyfWhYsIKKkAMejJuy50VTSfyfkrrJX2csg7fxJJt7vi+gXH8qxqH29C5GURaum")
            .SetVersion("2.28.0");

        manifest
            .DefineScript("trumbowyg")
            .SetDependencies("jquery-resizable")
            .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.28.0/dist/trumbowyg.min.js", "https://cdn.jsdelivr.net/npm/trumbowyg@2.28.0/dist/trumbowyg.js")
            .SetCdnIntegrity("sha384-O4OAGMDq5hnqt4/WQz+fW6yVgZ02jmw+Yf1j02zIgglnCYXf/7TmET8tFbrTN6u5", "sha384-dNxlebCuuiNWPhPBEd69nEAtkEWa7Z9IWkrL+OSmJ456dlu6TAASXgL72Bn4GGju")
            .SetVersion("2.28.0");

        manifest
            .DefineScript("trumbowyg-shortcodes")
            .SetDependencies("trumbowyg")
            .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg.shortcodes.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg.shortcodes.js")
            .SetVersion("1.0.0");

        manifest
            .DefineScript("trumbowyg-theme")
            .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg.theme.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg.theme.js")
            .SetDependencies("trumbowyg", "theme-head")
            .SetVersion("1.0.0");

        manifest
            .DefineStyle("trumbowyg-plugins")
            .SetDependencies("trumbowyg")
            .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg/trumbowyg-plugins.min.css", "~/OrchardCore.Resources/Styles/trumbowyg/trumbowyg-plugins.css")
            .SetVersion("2.28.0");

        manifest
            .DefineScript("trumbowyg-plugins")
            .SetDependencies("trumbowyg")
            .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg-plugins.min.js", "~/OrchardCore.Resources/Scripts/trumbowyg/trumbowyg-plugins.js")
            .SetVersion("2.28.0");

        manifest
            .DefineScript("credential-helpers")
            .SetUrl("~/OrchardCore.Resources/Scripts/credential-helpers.min.js", "~/OrchardCore.Resources/Scripts/credential-helpers.js")
            .SetVersion("1.0.0");

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
            .SetCdn("https://cdn.jsdelivr.net/npm/sortablejs@1.15.3/Sortable.min.js", "https://cdn.jsdelivr.net/npm/sortablejs@1.15.3/Sortable.js")
            .SetCdnIntegrity("sha384-/jkFGhPVLS9HIUzX09xB5W3coE5q1X5NXZA/PuOAdOaRxUPczlZmKzYEq9QcJnW0", "sha384-do1oujgtpAbjFRK6zLARg2zWqak7wvdRd7R7BHxErnVtSx8QUGQTv+QMTxsI+Bxq")
            .SetVersion("1.15.3");

        manifest
            .DefineScript("vuedraggable")
            .SetDependencies("vuejs", "Sortable")
            .SetUrl("~/OrchardCore.Resources/Scripts/vuedraggable.umd.min.js", "~/OrchardCore.Resources/Scripts/vuedraggable.umd.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/vuedraggable@2.24.3/dist/vuedraggable.umd.min.js", "https://cdn.jsdelivr.net/npm/vuedraggable@2.24.3/dist/vuedraggable.umd.js")
            .SetCdnIntegrity("sha384-qUA1xXJiX23E4GOeW/XHtsBkV9MUcHLSjhi3FzO08mv8+W8bv5AQ1cwqLskycOTs", "sha384-+jB9vXc/EaIJTlNiZG2tv+TUpKm6GR9HCRZb3VkI3lscZWqrCYDbX2ZXffNJldL9")
            .SetVersion("2.24.3");

        manifest
            .DefineScript("js-cookie")
            .SetUrl("~/OrchardCore.Resources/Scripts/js.cookie.min.js", "~/OrchardCore.Resources/Scripts/js.cookie.js")
            .SetCdn("https://cdn.jsdelivr.net/npm/js-cookie@3.0.5/dist/js.cookie.min.js", "https://cdn.jsdelivr.net/npm/js-cookie@3.0.5/dist/js.cookie.js")
            .SetCdnIntegrity("sha384-/vxhYfM1LENRhdpZ8dwEsQn/X4VhpbEZSiU4m/FwR+PVpzar4fkEOw8FP9Y+OfQN", "sha384-b1TD0tFP+Ao4jmFaQw9RQxezUooFrLdlqfDfoh1SKv5L3jG7dD44QiwD+UzckH8W")
            .SetVersion("3.0.5");

        manifest
            .DefineScript("monaco-loader")
            .SetUrl("~/OrchardCore.Resources/Scripts/monaco/vs/loader.js")
            .SetPosition(ResourcePosition.Last)
            .SetVersion(MonacoEditorVersion);

        manifest
            .DefineScript("monaco")
            .SetAttribute("data-tenant-prefix", _pathBase)
            .SetUrl("~/OrchardCore.Resources/Scripts/monaco/ocmonaco.js")
            .SetDependencies("monaco-loader")
            .SetVersion(MonacoEditorVersion);

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
