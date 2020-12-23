using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources
{
    public class ResourceManifest : IResourceManifestProvider
    {
        // CDNs
        private const string jsDelivrUrl = "https://cdn.jsdelivr.net/npm/";
        // Versions
        private const string codeMirrorVersion = "5.59.0";
        // URLs
        private const string codeMirrorUrl = jsDelivrUrl + "codemirror@" + codeMirrorVersion + "/";

        private ResourceDefinition DefineScript(ResourceManagement.ResourceManifest manifest, string scriptName, string scriptUrl, string scriptVersion, string dependency = "")
        {
            var minScriptUrl = scriptUrl.Replace(".js", ".min.js");
            var filename = scriptName.ToLower();

            var definition = manifest.DefineScript(scriptName)
                .SetUrl("~/OrchardCore.Resources/Scripts/" + filename + ".min.js", "~/OrchardCore.Resources/Scripts/" + filename + ".js")
                .SetCdn(minScriptUrl, scriptUrl)
                .SetVersion(scriptVersion);

            if (!string.IsNullOrEmpty(dependency))
            {
                definition.SetDependencies(dependency);
            }

            return definition;
        }

        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            DefineScript(manifest, "jQuery", "https://cdnjs.cloudflare.com/ajax/libs/jquery/3.5.1/jquery.js", "3.5.1")
                .SetCdnIntegrity("sha384-ZvpUoO/+PpLXR1lu4jmpXWu80pZlYUAfxl5NsBMWOEPSjUn/6Z/hRTt8+pR6L4N2", "sha384-/LjQZzcpTzaYn7qWqRIWYC5l8FWEZ2bIHIz0D73Uzba4pShEcdLdZyZkI4Kv676E");

            DefineScript(manifest, "jQuery.slim", "https://cdnjs.cloudflare.com/ajax/libs/jquery/3.5.1/jquery.slim.js", "3.5.1")
                .SetCdnIntegrity("sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj", "sha384-x6NENSfxadikq2gB4e6/qompriNc+y1J3eqWg3hAAMNBs4dFU303XMTcU3uExJgZ");

            DefineScript(manifest, "jQuery.easing", "https://cdnjs.cloudflare.com/ajax/libs/jquery-easing/1.4.1/jquery.easing.min.js", "1.4.1", "jQuery")
                .SetCdnIntegrity("sha384-leGYpHE9Tc4N9OwRd98xg6YFpB9shlc/RkilpFi0ljr3QD4tFoFptZvgnnzzwG4Q", "sha384-fwPA0FyfPOiDsglgAC4ZWmBGwpXSZNkq9IG+cM9HL4CkpNQo4xgCDkOIPdWypLMX");

            DefineScript(manifest, "jQuery-ui", "https://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js", "1.12.1", "jQuery")
                .SetCdnIntegrity("sha384-PtTRqvDhycIBU6x1wwIqnbDo8adeWIWP3AHmnrvccafo35E7oIvW7HPXn2YimvWu", "sha384-JPbtLYL10d/Z1crlc6GGGGM3PavCzzoUJ1UxH0bXHOfguWHQ6XAWrIzW+MBGGXe5");

            manifest
                .DefineStyle("jQuery-ui")
                .SetUrl("~/OrchardCore.Resources/Styles/jquery-ui.min.css", "~/OrchardCore.Resources/Styles/jquery-ui.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.css", "https://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.12.1/jquery-ui.css")
                .SetCdnIntegrity("sha384-VK/ia2DWrvtO05YDcbWI8WE3WciOH0RhfPNuRJGSa3dpAs5szXWQuCnPNv/yzpO4", "sha384-7UG1JWOW1s2Zo3gDfHmL9dAVD/QS57qS+pRAq3icsyDwzlkPRQlk4PDD0bySIiQq")
                .SetVersion("1.12.1");

            manifest
                .DefineScript("jQuery-ui-i18n")
                .SetDependencies("jQuery-ui")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-ui-i18n.min.js", "~/OrchardCore.Resources/Scripts/jquery-ui-i18n.js")
                .SetCdn("https://code.jquery.com/ui/1.7.2/i18n/jquery-ui-i18n.min.js", "https://code.jquery.com/ui/1.7.2/i18n/jquery-ui-i18n.min.js")//https://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.9.2/i18n/jquery-ui-i18n.min.js
                .SetCdnIntegrity("sha384-0rV7y4NH7acVmq+7Y9GM6evymvReojk9li+7BYb/ug61uqPSsXJ4uIScVY+N9qtd", "sha384-0rV7y4NH7acVmq+7Y9GM6evymvReojk9li+7BYb/ug61uqPSsXJ4uIScVY+N9qtd")
                .SetVersion("1.7.2");

            DefineScript(manifest, "popper", "https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.js", "1.16.1")
                .SetCdnIntegrity("sha384-9/reFTGAW83EW2RDu2S0VKaIzap3H66lZH81PoYlFhbGU+6BZp6G7niu735Sk7lN", "sha384-cpSm/ilDFOWiMuF2bj03ZzJinb48NO9IGCXcYDtUzdP5y64Ober65chnoOj1XFoA");

            DefineScript(manifest, "bootstrap", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.js", "4.5.3")
                .SetDependencies("jQuery", "popper")
                .SetCdnIntegrity("sha384-w1Q4orYjBQndcko6MimVbzY0tgp4pWB4lZ7lr30WKz0vr/aWKhXdBNmNb5D92v7s", "sha384-UACdLYI/ku0J5/hhojLBvchW9KNT++8VmZQW9cJDVIEth0APvgNO4qSPpIxp8F9T");

            DefineScript(manifest, "bootstrap-bundle", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.bundle.js", "4.5.3")
                .SetCdnIntegrity("sha384-ho+j7jyWK8fNQe+A12Hb8AhRq26LrZ/JpcUGGOn+Y7RsweNrtN/tE3MoK7ZeZDyx", "sha384-q/QDYob/o4XN/fpFWt7AEU+hWQLAc0FCviSzMpKqarlw2VVqk2mgn971KCJ64zpo");

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.min.css", "~/OrchardCore.Resources/Styles/bootstrap.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.min.css", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.css")
                .SetCdnIntegrity("sha384-TX8t27EcRE3e/ihU7zmQxVncDAy5uIKz4rEkgIXeMed4M0jlfIDPvg6uqKI2xXr2", "sha384-Ro2DNoUODgrLmRM7WL/mbXZ1D6WaudEiPPceIZTfzZrTahyJAxLMj5TF2RQwrpiG")
                .SetVersion("4.5.3");

            manifest
                .DefineScript("bootstrap-select")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-select.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-select.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.18/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.18/dist/js/bootstrap-select.js")
                .SetCdnIntegrity("sha384-x8fxIWvLdZnkRaCvkDXOlxd6UP+qga975FjnbRp6NRpL9jjXjY9TwF9y7z4AccxS", "sha384-6BZTOUHC4e3nWcy5gveLqAu52vwy5TX8zBIvvfZFVDzIjYDgprdXRMK/hsypxdpQ")
                .SetVersion("1.13.18");

            manifest
                .DefineScript("bootstrap-slider")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-slider.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-slider.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/bootstrap-slider/11.0.2/bootstrap-slider.min.js", "https://cdnjs.cloudflare.com/ajax/libs/bootstrap-slider/11.0.2/bootstrap-slider.js")
                .SetCdnIntegrity("sha384-lZLZ1uMNIkCnScGXQrJ+PzUR2utC/FgaxJLMMrQD3Fbra1AwGXvshEIedqCmqXTM", "sha384-3kfvdN8W/a8p/9S6Gy69uVsacwuNxyvFVJXxZa/Qe00tkNfZw63n/4snM1u646YU")
                .SetVersion("11.0.2");

            manifest
                .DefineStyle("codemirror")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/codemirror.min.css", "~/OrchardCore.Resources/Styles/codemirror/codemirror.css")
                .SetCdn(codeMirrorUrl + "lib/codemirror.min.css", codeMirrorUrl + "lib/codemirror.css")
                .SetCdnIntegrity("sha384-h3l8qwg0s18FbBn7yQprJyfhea0JR5bHVKUADDjGVW6JmV82nU0BqqJUBLSefoXq", "sha384-me3GYw39+VnACscdDXhP/b1IARe/OQVQhFGmJaZpMi9PappkEGC93cl+iVBotdjX")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn(codeMirrorUrl + "codemirror.min.js", codeMirrorUrl + "codemirror.js")
                .SetCdnIntegrity("sha384-geaVDJfYxAAEM+WjtPT8vRlokdXA7vrGtq2W6vPpDWEaYUT1v0pIaLiH0vfuB4Qc", "sha384-Qe4dp0Ge4hHQeRFyd4jlDr5mU5s9WYkeEvU0fANhRhuGmtB5YEr9ACwxqbt8R2Un")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css", "~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css")
                .SetCdn(codeMirrorUrl + "addon/display/fullscreen.min.css", codeMirrorUrl + "addon/display/fullscreen.css")
                .SetCdnIntegrity("sha384-uuIczW2AGKADJpvg6YiNBJQWE7duDkgQDkndYEsbUGaLm8SPJZzlly6hcEo0aTlW", "sha384-+glu1jsbG+T5ocmkeMvIYh5w07IXKxmJZaCdkNbVfpEr3xi+M0gopFSR/oLKXxio")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineStyle("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.css")
                .SetCdn(codeMirrorUrl + "addon/hint/show-hint.min.css", codeMirrorUrl + "addon/hint/show-hint.css")
                .SetCdnIntegrity("sha384-6WPQ0Et1pejdxD4Wo5iMLAbHevsy831YEIHf89b0w5XEfsp1ZdugT38rRfdnSkTj", "sha384-ZZbLvEvLoXKrHo3Tkh7W8amMgoHFkDzWe8IAm1ZgxsG5y35H+fJCVMWwr0YBAEGA")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-selection-active-line")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.js")
                .SetCdn(codeMirrorUrl + "addon/selection/active-line.min.js", codeMirrorUrl + "addon/selection/active-line.min.js")
                .SetCdnIntegrity("sha384-ltcHPDzeKabhtrWLWOso039GpBKMW42wZOfLibeT1gyguorS6LhR3/tjOdTG5qm4", "sha384-kKz13r+qZMgTNgXROGNHQ0/0/J1FtvIvRZ9yjOHo1YLUCd+KF8r9R+su/B+f6C0U")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-display-autorefresh")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
                .SetCdn(codeMirrorUrl + "addon/display/autorefresh.min.js", codeMirrorUrl + "addon/display/autorefresh.js")
                .SetCdnIntegrity("sha384-NBTrjfZEptRZhiWm8iJETqVVYWVH0FT+BmOC3QQEnUGmZaBqryyGb6wr4n98Q0S+", "sha384-5wrQkkCzj5dJInF+DDDYjE1itTGaIxO+TL6gMZ2eZBo9OyWLczkjolFX8kixX/9X")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn(codeMirrorUrl + "addon/display/fullscreen.min.js", codeMirrorUrl + "addon/display/fullscreen.js")
                .SetCdnIntegrity("sha384-a8HUP5qgFf7gCxS+2A7NqHEoL1HSyBJPPEIaT6v8q+DIQBd/PyWXHinhoucUzLVQ", "sha384-kuwHYEheDAp0NlPye2VXyxJ6J54CeVky9tH+e0mn6/gP4PJ50MF4FGKkrGAxIyCW")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-edit-closetag")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn(codeMirrorUrl + "addon/edit/closetag.min.js", codeMirrorUrl + "addon/edit/closetag.js")
                .SetCdnIntegrity("sha384-GmzWefDpo9oYhOLBBYaxr20AN/y5eLPDApkPnlqlVo7WHTxEe+ZCJi2MywalxT4B", "sha384-jR3Qxv7tHnv4TLLymC5s7Tl3aQGWNayqUHHBNxnnA/NjIyewLGSmNPQuDcMxnPKY")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.js")
                .SetCdn(codeMirrorUrl + "addon/hint/show-hint.min.js", codeMirrorUrl + "addon/hint/show-hint.js")
                .SetCdnIntegrity("sha384-qGi6kqccz6wLDsteKUipfWax8rb1T3MGBjHGrgCS1fTMw2x/z4vF5v/tUI6FojNB", "sha384-VIjTLfyf8TDATLORaNeFgbAzeAy9ZbmWEkjdQhJc5JyrdrnBbiTrixhYCizHe7vL")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-hint-sql-hint")
                .SetDependencies("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.js")
                .SetCdn(codeMirrorUrl + "addon/hint/sql-hint.min.js", codeMirrorUrl + "addon/hint/sql-hint.js")
                .SetCdnIntegrity("sha384-jzBqyNLLcJ6q5yPvDDsAvalRlvigz7kV6Zi7uJ7p9KMz2qMNKQojEVmxjuD6TGge", "sha384-CWV9H0BqWpAc3P54hnDdDdzwXIZQn03V/Cn1mJIp/EMWDqHKuMtFNeY9LHpI1VIQ")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn(codeMirrorUrl + "addon/mode/multiplex.min.js", codeMirrorUrl + "addon/mode/multiplex.js")
                .SetCdnIntegrity("sha384-WvAaxIYSi0+NgUpyxZxXCfbheBffaJx8hvDHduVumetfTHt1pyqsbTrIp68BXJf4", "sha384-xF886YuXgjqY02JjHuoV+B+SZYB4DYq7YUniuin/yCqJdxYpXomTlOfmiSoSNpwQ")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn(codeMirrorUrl + "addon/mode/simple.min.js", codeMirrorUrl + "addon/mode/simple.js")
                .SetCdnIntegrity("sha384-Bhk/hy0Jz0p0vhqtgnyaZWiKqV6B/39GQr66f8wQUi8wwNP4nYwkCLNVQpqfR0AV", "sha384-ntjFEzI50GYBTbLGaOVgBt97cxp74jfCqMDmZYlGWk8ZZp2leFMJYOp85T3tOeG9")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-css")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.js")
                .SetCdn(codeMirrorUrl + "mode/css/css.min.js", codeMirrorUrl + "mode/css/css.js")
                .SetCdnIntegrity("sha384-RRQkcW3+aB8IG7IfJXXuvpnp+zzjpP5HT2A5X6OCfoZY34MiL5n7xO8XPlza0KgD", "sha384-iLRwl/wmrsnZGJIhA5VE7Nn7HrI/QsDdjB4GCqKnZ82i+jVHqaCSXw2GeuA6a7P+")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-htmlmixed")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.js")
                .SetCdn(codeMirrorUrl + "mode/htmlmixed/htmlmixed.min.js", codeMirrorUrl + "mode/htmlmixed/htmlmixed.js")
                .SetCdnIntegrity("sha384-4akYRGS08BCmZf+RlSzKG2OsaJUcXX1Nh8ZZtPQL2TMbhG/67YC1orG3MPoFEV/y", "sha384-vM5dpc39AxwLWe2WC/4ZNAw81WDwmu5CoPw9uw7XfDMLtUKrHFEsz4ofeaRWVIEP")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn(codeMirrorUrl + "mode/javascript/javascript.min.js", codeMirrorUrl + "mode/javascript/javascript.js")
                .SetCdnIntegrity("sha384-iNxcerSwr8AT+oLZ7r2NJPD3vBOktMWGRF4BWXzBsNd2v42YvoMHiC9Lar1pCUwZ", "sha384-F0vjx9V4PMxZrDjn4GxLQ4101kEF5Lvey5J+99uF+MgRaDMtNGOVIbJn8wIQ6bqH")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn(codeMirrorUrl + "mode/sql/sql.min.js", codeMirrorUrl + "mode/sql/sql.js")
                .SetCdnIntegrity("sha384-Yrd0cLaV+YsVyGE8r228/0b6Lkr9NSwYwAEov/Dxt5NhHAEb1b0IRUjgJNiJAtBE", "sha384-D+uYw7olvmCEyME9PHzGRne1Sh8+XTk84/JZinlhDTutXBH45AdThw4uLAAt4vIm")
                .SetVersion(codeMirrorVersion);

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn(codeMirrorUrl + "mode/xml/xml.min.js", codeMirrorUrl + "mode/xml/xml.js")
                .SetCdnIntegrity("sha384-wurFCCBbIrLPSPi+hTuH4O0NuB5oDVqb553r3TVPS49DYV8/dkjWAgKNOPKDfcrL", "sha384-EwUMa7GJX1216cyeag/TZuBXBbWL5EChhsbXe5A71TD2H9TDEq9n6FL2haWqk+im")
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
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/css/all.css")
                .SetCdnIntegrity("sha384-vp86vTRFVJgpjF9jiIGPEEqYqlDwgyBgEF109VFjmqGmIY/Y4HV4d3Gp2irVfcrp", "sha384-onlHyXkx02a/ZTmCZVUQDO+4JnJwhOvHXBLvN1aal24ABfJ0VDUt7RyzsmpPswfe")
                .SetVersion("5.15.1");

            manifest
                .DefineScript("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/js/all.js")
                .SetCdnIntegrity("sha384-9/D4ECZvKMVEJ9Bhr3ZnUAF+Ahlagp1cyPC7h5yDlZdXs4DQ/vRftzfd+2uFUuqS", "sha384-vkhNV6UI0lzGY+LU1vEFOQTnBcvaEr/ExwLUaZJmpqsmRZu5LDxNB+yEQiXIqCg9")
                .SetVersion("5.15.1");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.15.1/js/v4-shims.js")
                .SetCdnIntegrity("sha384-IEHK9LKBXJdi7Y/gik7R6VYPuwx8hMiwQuaOh7BQUQ9rKmWr2N04KYFdmt5Xi0qG", "sha384-ABbiFGmmXw9KRBNur7lAKeUU6BzxXpcF3REBd1igMGOmBqFzU0Og92yPeRbTgMLa")
                .SetVersion("5.15.1");

            manifest
                .DefineScript("jquery-resizable")
                .SetDependencies("resizable-resolveconflict")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-resizable.min.js", "~/OrchardCore.Resources/Scripts/jquery-resizable.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/jquery-resizable-dom@0.35.0/dist/jquery-resizable.min.js")
                .SetCdnIntegrity("sha384-1LMjDEezsSgzlRgsyFIAvLW7FWSdFIHqBGjUa+ad5EqtK1FORC8XpTJ/pahxj5GB", "sha384-0yk9X0IG0cXxuN9yTTkps/3TNNI9ZcaKKhh8dgqOEAWGXxIYS5xaY2as6b32Ov3P")
                .SetVersion("0.35.0");

            manifest
                .DefineScript("resizable-resolveconflict")
                .SetDependencies("jQuery-ui")
                .SetUrl("~/OrchardCore.Resources/Scripts/resizable-resolveconflict.min.js", "~/OrchardCore.Resources/Scripts/resizable-resolveconflict.js")
                .SetVersion("2.23.0");

            manifest
                .DefineStyle("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg.min.css", "~/OrchardCore.Resources/Styles/trumbowyg.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.23.0/dist/ui/trumbowyg.min.css", "https://cdn.jsdelivr.net/npm/trumbowyg@2.23.0/dist/ui/trumbowyg.css")
                .SetCdnIntegrity("sha384-SFeSoDnGCqwq9pGDKTu07ju9Jvj+/Rphyn6ZOrFEAWv/39BDsEsmvq/E55Jym2qW", "sha384-VDzdVRx3LkvZ4Nu0K+uMjtsQcjVfwyq7QzO/2k848iRdPLYFWtkaLVhmKCMmDICs")
                .SetVersion("2.23.0");

            manifest
                .DefineScript("trumbowyg")
                .SetDependencies("jquery-resizable")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg.js", "~/OrchardCore.Resources/Scripts/trumbowyg.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.23.0/dist/trumbowyg.min.js", "https://cdn.jsdelivr.net/npm/trumbowyg@2.23.0/dist/trumbowyg.js")
                .SetCdnIntegrity("sha384-urIqQtzqgJuOaj/Ol4GZq0tuivHePKpi1qgq9dO76Kgl3w798Drshofda/dYF6oQ", "sha384-E8+fkaJ9Jey+LrKvWi1OpCOvOp3b5nhKPxlEEG1IDlxonsxKnjKEfrLUT/iFhQEo")
                .SetVersion("2.23.0");

            manifest
                .DefineScript("trumbowyg-shortcodes")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg.shortcodes.js", "~/OrchardCore.Resources/Scripts/trumbowyg.shortcodes.min.js")
                .SetVersion("1.0.0");

            manifest
                .DefineStyle("trumbowyg-plugins")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg-plugins.min.css", "~/OrchardCore.Resources/Styles/trumbowyg-plugins.css")
                .SetVersion("2.23.0");

            manifest
                .DefineScript("trumbowyg-plugins")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg-plugins.js", "~/OrchardCore.Resources/Scripts/trumbowyg-plugins.js")
                .SetVersion("2.23.0");

            manifest
                .DefineScript("vuejs")
                .SetUrl("~/OrchardCore.Resources/Scripts/vue.min.js", "~/OrchardCore.Resources/Scripts/vue.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/vue@2.6.11/dist/vue.min.js", "https://cdn.jsdelivr.net/npm/vue@2.6.11/dist/vue.js")
                .SetCdnIntegrity("sha384-OZmxTjkv7EQo5XDMPAmIkkvywVeXw59YyYh6zq8UKfkbor13jS+5p8qMTBSA1q+F", "sha384-+jvb+jCJ37FkNjPyYLI3KJzQeD8pPFXUra3B/QJFqQ3txYrUPIP1eOfxK4h3cKZP")
                .SetVersion("2.6.11");

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
                .SetCdn("https://cdn.jsdelivr.net/npm/sortablejs@1.10.2/Sortable.min.js", "https://cdn.jsdelivr.net/npm/sortablejs@1.10.2/Sortable.js")
                .SetCdnIntegrity("sha384-6qM1TfKo1alBw3Uw9AWXnaY5h0G3ScEjxtUm4TwRJm7HRmDX8UfiDleTAEEg5vIe", "sha384-lNRluF0KgEfw4KyH2cJAoBAMzRHZVp5bgBGAzRxHeXoFqb5admHjitlZ2dmspHmC")
                .SetVersion("1.10.2");

            manifest
                .DefineScript("vuedraggable")
                .SetDependencies("vuejs", "Sortable")
                .SetUrl("~/OrchardCore.Resources/Scripts/vuedraggable.umd.min.js", "~/OrchardCore.Resources/Scripts/vuedraggable.umd.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/vuedraggable@2.24.3/dist/vuedraggable.umd.min.js", "https://cdn.jsdelivr.net/npm/vuedraggable@2.24.3/dist/vuedraggable.umd.js")
                .SetCdnIntegrity("sha384-qUA1xXJiX23E4GOeW/XHtsBkV9MUcHLSjhi3FzO08mv8+W8bv5AQ1cwqLskycOTs", "sha384-+jB9vXc/EaIJTlNiZG2tv+TUpKm6GR9HCRZb3VkI3lscZWqrCYDbX2ZXffNJldL9")
                .SetVersion("2.24.3");

            manifest
                .DefineScript("jscookie")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/js.cookie.min.js", "~/OrchardCore.Resources/Scripts/js.cookie.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/js-cookie/2.2.1/js.cookie.min.js", "https://cdnjs.cloudflare.com/ajax/libs/js-cookie/2.2.1/js.cookie.js")
                .SetCdnIntegrity("sha384-eITc5AorI6xzkW7XunGaNrcA0l6qrU/kA/mOhLQOC5thAzlHSClQTOecyzGK6QXK", "sha384-qYkL05PP6ICwBjU1X95J3yIhrm7w3efbzz0r1oti35uPxRjXP6t5B8gP0xNuOmdt")
                .SetVersion("2.2.1");
        }
    }
}
