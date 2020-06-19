using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.min.js", "~/OrchardCore.Resources/Scripts/jquery.js")
                .SetCdn("https://code.jquery.com/jquery-3.4.1.min.js", "https://code.jquery.com/jquery-3.4.1.js")
                .SetCdnIntegrity("sha384-vk5WoKIaW/vJyUAd9n/wmopsmNhiy+L2Z+SBxGYnUkunIxVxAv/UtMOhba/xskxh", "sha384-mlceH9HlqLp7GMKHrj5Ara1+LvdTZVMx4S1U43/NxCvAkzIo8WJ0FE7duLel3wVo")
                .SetVersion("3.4.1");

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.slim.min.js", "~/OrchardCore.Resources/Scripts/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.4.1.slim.min.js", "https://code.jquery.com/jquery-3.4.1.slim.js")
                .SetCdnIntegrity("sha384-J6qa4849blE2+poT4WnyKhv5vZF5SrPo0iEjwBvKU7imGFAV0wwj1yYfoRSJoZ+n", "sha384-teRaFq/YbXOM/9FZ1qTavgUgTagWUPsk6xapwcjkrkBHoWvKdZZuAeV8hhaykl+G")
                .SetVersion("3.4.1");

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
                .SetUrl("~/OrchardCore.Resources/Scripts/popper.min.js", "~/OrchardCore.Resources/Scripts/popper.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js", "https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.js")
                .SetCdnIntegrity("sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo", "sha384-EsqqCR7beeX9mWjsrB8ySgz3pKDhWr3OgqnudRtew5RApIIhEN6/qqiPM99Lk9qM")
                .SetVersion("1.16.0");

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery", "popper")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js", "https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.js")
                .SetCdnIntegrity("sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6", "sha384-IGTd+U9dY/fgJBXURnLtTaaxga6WSJj46heDWHy/GPu8yyuP3HERqWszUMyWPeWR")
                .SetVersion("4.4.1");

            manifest
                .DefineScript("bootstrap-bundle")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.bundle.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.bundle.min.js", "https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-6khuMg9gaYr5AxOqhkVIODVIvm9ynTT5J4V1cfthmT+emCG6yVmEZsRHdxlotUnm", "sha384-FhLadiSx562CHg1MeZgAwnrYbHS58gnpCpvPQn6Sv7SqLRCyEKRXE1if+Zx36XNm")
                .SetVersion("4.4.1");

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.min.css", "~/OrchardCore.Resources/Styles/bootstrap.css")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css", "https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.css")
                .SetCdnIntegrity("sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh", "sha384-vXOtxoYb1ilJXRLDg4YD1Kf7+ZDOiiAeUwiH9Ds8hM8Paget1UpGPc/KlaO33/nt")
                .SetVersion("4.4.1");

            manifest
                .DefineStyle("bootstrap-select")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap-select.min.css", "~/OrchardCore.Resources/Styles/bootstrap-select.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.17/dist/css/bootstrap-select.min.css", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.17/dist/css/bootstrap-select.css")
                .SetCdnIntegrity("sha384-4VrsKsu8ei1Ey+aaM9aKxZf6kwZAzi9VAule5Owa012ZwHIw4XdZF3DRV3TL29po", "sha384-P8wP2x41Lu1DByhfELrakT0P9b5RvCFY7gelQ16gEef2OGFG7q/k6ytMEx8fvrFb")
                .SetVersion("1.13.17");

            manifest
                .DefineScript("bootstrap-select")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-select.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-select.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.17/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.17/dist/js/bootstrap-select.js")
                .SetCdnIntegrity("sha384-vZHItSjiaeaa+fcNthtymKys5rxEOQ30i0gepK/bHdxByApBXUVkUvJwmaorQHW+", "sha384-N18QyApph3arAgOSkvDBbC4p2Nmrg5yqThKLK7l9n2FOpbodYX0vW0ediUB7Po6n")
                .SetVersion("1.13.17");

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/codemirror.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/codemirror.js")
                .SetCdnIntegrity("sha384-/c3bE9vjYQnzNoSBQkxZzsqN6vvWxH/7M6I2MOnvgwEuvHDUPEFTgHW75I68aJBu", "sha384-LL4jRPUqSgcatvsRrK1jbaOQKkpx8G/yCAMQhIcAadYpXoTvrnef4oZ4xYWypN+u")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-addon-display-autorefresh")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/addon/display/autorefresh.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/addon/display/autorefresh.js")
                .SetCdnIntegrity("sha384-pn83o6MtS8kicn/sV6AhRaBqXQ5tau8NzA2ovcobkcc1uRFP7D8CMhRx231QwKST", "sha384-5wrQkkCzj5dJInF+DDDYjE1itTGaIxO+TL6gMZ2eZBo9OyWLczkjolFX8kixX/9X")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/addon/mode/multiplex.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/addon/mode/multiplex.js")
                .SetCdnIntegrity("sha384-sTw3lU1fIEXG/qNbuuh7lpitFgxSwaReQLtKo8gVCzKz/j3yOk8whDk4NiG5Cnvs", "sha384-xF886YuXgjqY02JjHuoV+B+SZYB4DYq7YUniuin/yCqJdxYpXomTlOfmiSoSNpwQ")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/addon/mode/simple.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/addon/mode/simple.js")
                .SetCdnIntegrity("sha384-DbFZlXwgV4md0LMM7aSVdbUaCldkPh4QXILJ/kbD9ZZiC3gQ+E9SjHG++B3PvCr1", "sha384-ntjFEzI50GYBTbLGaOVgBt97cxp74jfCqMDmZYlGWk8ZZp2leFMJYOp85T3tOeG9")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-mode-css")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/css/css.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/css/css.js")
                .SetCdnIntegrity("sha384-7jWEUNIiKACsg4CC/mH6GAi028ses/AR9YrcDgggjvCNbAMPGzAntlk7wZCcbFVK", "sha384-EOd2XWza2wnlyji1uXvCHhzv8KidkG8oG2DUP104jMguvgGdf62zxdt6l/f8pVp8")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-mode-htmlmixed")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/htmlmixed/htmlmixed.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/htmlmixed/htmlmixed.js")
                .SetCdnIntegrity("sha384-sbLNpMWktagHnz1dYX5ffxjnU0+k5cGjZTg2QDE64Gi/fZ28b0USo6CZvaEjyKKl", "sha384-fxS517EbIB2hObIbibF9AuQAFnAlwZgIsy2zpeCqpw4yIkJRfNOLSe0JHu7H7ULE")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/javascript/javascript.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/javascript/javascript.js")
                .SetCdnIntegrity("sha384-ULvdaxv0DzYHo7+N+vTqPOkrb+c0HjRwkwMmYs6Y6W8nE2wZJ5AEGhHels3j20eQ", "sha384-L14nIDN0efNBLMdKgOpiMNUCywiIiGZb1wjrFlFxA5o5/2VDYZIVOsGCcivF28lp")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/sql/sql.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/sql/sql.js")
                .SetCdnIntegrity("sha384-FFeBZmn3X1Caguvl8tB1ElFsA53NoCMUGNk2/rctjiCMj0awY0U0xFd5BjpVGdE4", "sha384-xr/+8UfxBnDrmtQUJB2HPXTe2ckjm/q6uwhED510ZKDjqfBtNTTOOTsy/tezyrQu")
                .SetVersion("5.54.0");

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/xml/xml.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/mode/xml/xml.js")
                .SetCdnIntegrity("sha384-27kgmf2cIJ9bclpvyUN4SgnnYnW7Wu0MiGYpm+fmNQhcREFI7mq0C9ehuxYuwqgy", "sha384-z8O1dAcb8dxGYbhmbzbJ41xAUlle9jfZpok46e8s9+Tyu9bKxx742hiNqVJ0D6cL")
                .SetVersion("5.54.0");

            manifest
                .DefineStyle("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.css", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/codemirror.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.54.0/codemirror.css")
                .SetCdnIntegrity("sha384-IOz5QFCDEKXQEiYKlSOUsHhcrCpRO2Vn9EcidoFf6olMAZVXnLgcoQclMBoksUOa", "sha384-mVNiRdLEUGBMLPITebcG8Bc5wJW9HT/x0TiXmad0ZMPF9g0CMX2IYLNHqGjCmUsU")
                .SetVersion("5.54.0");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Styles/font-awesome.min.css", "~/OrchardCore.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.min.css", "~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.0/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.0/css/all.css")
                .SetCdnIntegrity("sha384-Bfad6CLCknfcloXFOyFnlgtENryhrpZCe29RTifKEixXQZ38WheV+i/6YWSzkz3V", "sha384-I4s88/QBf6OKVFMRRyjRY9wYdRoEO/PnNLQ1xY+G0OQfntKp8FxRsOg9qjmtbnvL")
                .SetVersion("5.13.0");

            manifest
                .DefineScript("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.0/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.0/js/all.js")
                .SetCdnIntegrity("sha384-ujbKXb9V3HdK7jcWL6kHL1c+2Lj4MR4Gkjl7UtwpSHg/ClpViddK9TI7yU53frPN", "sha384-Z4FE6Znluj29FL1tQBLSSjn1Pr72aJzuelbmGmqSAFTeFd42hQ4WHTc0JC30kbQi")
                .SetVersion("5.13.0");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.0/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.0/js/v4-shims.js")
                .SetCdnIntegrity("sha384-XrjUu+3vbvZoLzAa5lC50XIslajkadQ9kvvTTq0odZ+LCoNEGqomuCdORdHv6wZ6", "sha384-lFg/ndztJNIxHOFryzbqXA8p6T82IJwwux+3oNIVGCuoMoLx8UbAOEf63Tt507Aq")
                .SetVersion("5.13.0");

            manifest
                .DefineScript("jquery-resizable")
                .SetDependencies("resizable-resolveconflict")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-resizable.min.js", "~/OrchardCore.Resources/Scripts/jquery-resizable.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/jquery-resizable-dom@0.35.0/dist/jquery-resizable.min.js")
                .SetCdnIntegrity("sha384-1LMjDEezsSgzlRgsyFIAvLW7FWSdFIHqBGjUa+ad5EqtK1FORC8XpTJ/pahxj5GB", "sha384-0yk9X0IG0cXxuN9yTTkps/3TNNI9ZcaKKhh8dgqOEAWGXxIYS5xaY2as6b32Ov3P")
                .SetVersion("0.35.0");

            manifest
                .DefineScript("resizable-resolveconflict")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/resizable-resolveconflict.min.js", "~/OrchardCore.Resources/Scripts/resizable-resolveconflict.js")
                .SetVersion("2.21.0");

            manifest
                .DefineStyle("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg.min.css", "~/OrchardCore.Resources/Styles/trumbowyg.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.21.0/dist/ui/trumbowyg.min.css", "https://cdn.jsdelivr.net/npm/trumbowyg@2.21.0/dist/ui/trumbowyg.css")
                .SetCdnIntegrity("sha384-/QRP5MyK1yCOLeUwO9+YXKUDNFKRuzDjVDW+U8RnsI/9I3+p538CduXmiLfzWUY4", "sha384-helPIukt/ukxd7K8G/hg2Hgi5Zt2V5khBjNiQjpRUPE/mV/7I3Cym7fVGwol5PzR")
                .SetVersion("2.21.0");

            manifest
                .DefineScript("trumbowyg")
                .SetDependencies("jquery-resizable")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg.js", "~/OrchardCore.Resources/Scripts/trumbowyg.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/trumbowyg@2.21.0/dist/trumbowyg.min.js", "https://cdn.jsdelivr.net/npm/trumbowyg@2.21.0/dist/trumbowyg.js")
                .SetCdnIntegrity("sha384-XrYMLffzTUgFmXcXtkSWBUpAzHQzzDOXM96+7pKkOIde9oUDWNb72Ij7K06zsLTV", "sha384-I0b1bxE3gmTi8+HE5xlvTLLehif/97lNC+tk2dGrln7dtdQ/FasdZRDbXAg3rBus")
                .SetVersion("2.21.0");

            manifest
                .DefineStyle("trumbowyg-plugins")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Styles/trumbowyg-plugins.min.css", "~/OrchardCore.Resources/Styles/trumbowyg-plugins.css")
                .SetVersion("2.21.0");

            manifest
                .DefineScript("trumbowyg-plugins")
                .SetDependencies("trumbowyg")
                .SetUrl("~/OrchardCore.Resources/Scripts/trumbowyg-plugins.js", "~/OrchardCore.Resources/Scripts/trumbowyg-plugins.js")
                .SetVersion("2.21.0");

            manifest
                .DefineScript("vuejs")
                .SetUrl("~/OrchardCore.Resources/Scripts/vue.js", "~/OrchardCore.Resources/Scripts/vue.js")
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
                .SetCdn("https://cdn.jsdelivr.net/npm/vuedraggable@2.23.2/dist/vuedraggable.umd.min.js", "https://cdn.jsdelivr.net/npm/vuedraggable@2.23.2/dist/vuedraggable.umd.js")
                .SetCdnIntegrity("sha384-76x2+A0FtaKEJTehctEO1ZZD7nUoFvLP4cEa2yCznMsOjj0SvK2rd24FgP9EnuzJ", "sha384-xdeWopJ4Lu/6a41wOnXJ7yjwWe7TrZ0RDREHDKk8OpnzYZSrwxg3r8MqAbog8Y0l")
                .SetVersion("2.23.2");
        }
    }
}
