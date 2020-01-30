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
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.12/dist/css/bootstrap-select.min.css", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.12/dist/css/bootstrap-select.css")
                .SetCdnIntegrity("sha384-BJPGVhka8+B49CO2MFRKLZ0fD0v142Ssd+px+a64YvT+EoCupeZSxIxPvxafQ4cJ", "sha384-JSuPufd1/O23uRXbGqmXAyb+8CKGRtjoK394uIUl1BbPpnBXoM1GYMY7wYcqeRSR")
                .SetVersion("1.13.12");

            manifest
                .DefineScript("bootstrap-select")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-select.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-select.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.12/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.12/dist/js/bootstrap-select.js")
                .SetCdnIntegrity("sha384-ykzduUaBYjweaCG/roIizm54PztxJiXT7XLC6dkluArvYbvp74xjRWxyzmg7u5/4", "sha384-XDvvoI/zwjnYA93MESVbVwnq0jOMdG6+6b9EDFOkguHI0EaOcA1OGTzg4OIUV5do")
                .SetVersion("1.13.12");

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/codemirror.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/codemirror.js")
                .SetCdnIntegrity("sha384-KHtNpLjgn/PbrXlrGBrcUDxHcP2TuaNrMIWhKPo9PBN4nn2QastGJv5qKWKm2uCp", "sha384-HkSY0HXZ8maBGMuG/fdzmSB70ilqjnRaVtjf5un224lcGnzMXtqPvpylh8Ko8Vgd")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-addon-display-autorefresh")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/addon/display/autorefresh.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/addon/display/autorefresh.js")
                .SetCdnIntegrity("sha384-RJTX2U27s6bG/uQqwUnSXT4c8lTmHkfEbmb9d6KRrNW1qJuPdJs6r9SGFcPhRrYh", "sha384-5wrQkkCzj5dJInF+DDDYjE1itTGaIxO+TL6gMZ2eZBo9OyWLczkjolFX8kixX/9X")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/addon/mode/multiplex.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/addon/mode/multiplex.js")
                .SetCdnIntegrity("sha384-VyJaGZ3qxy1XF9EP7YBdjQ75CK5ibrhBTxPiRWd13c1mZ3PnOE0knauoqLVvpPG4", "sha384-xF886YuXgjqY02JjHuoV+B+SZYB4DYq7YUniuin/yCqJdxYpXomTlOfmiSoSNpwQ")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/addon/mode/simple.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/addon/mode/simple.js")
                .SetCdnIntegrity("sha384-Vge7WgleZMeEAgz0ayHaxG4RROPv4eOb2qc/AkJzlXU413oNqVYzVoCpz/6Xa+yy", "sha384-ntjFEzI50GYBTbLGaOVgBt97cxp74jfCqMDmZYlGWk8ZZp2leFMJYOp85T3tOeG9")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/javascript/javascript.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/javascript/javascript.js")
                .SetCdnIntegrity("sha384-9PfA9qrQeVVDyGIaJm3ZAN6tqKlhgIWkcHEgrAbGYDK0UjRPhYA3PWWdc1S7zo4i", "sha384-2yiSY+7t9iOgbVp5LMNNz+Msni9LEluBKReEy0yWbr6qwsPWHKcr7h80glSW7ePp")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/sql/sql.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/sql/sql.js")
                .SetCdnIntegrity("sha384-2nSGFlwplo1Jn7F5swxFzBRrwYLBd7VI+cF/YS7H+KdvQ2LjLf6xcvxa2C2gZSch", "sha384-YpO66H86iHfEAU58XAz43HJMfEdhAq5/8rK298QDuNbe5MV4m55bXT4B+FIUhhdI")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/xml/xml.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/xml/xml.js")
                .SetCdnIntegrity("sha384-Td+NIu0gS+YWPRtXUNyWuDif8s6NPI4W0PlFk37Sdoi9em/G6IZV3KrSjtj2WnKK", "sha384-Fwj4mOSYqdnz4tUEeELhXbwTJWq+aGpRvHnE7XNaUquMFhMkj8UFX5rvNQD2zHlQ")
                .SetVersion("5.48.4");

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/graphql/graphql.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/mode/xml/xml.js")
                .SetCdnIntegrity("sha384-Td+NIu0gS+YWPRtXUNyWuDif8s6NPI4W0PlFk37Sdoi9em/G6IZV3KrSjtj2WnKK", "sha384-Fwj4mOSYqdnz4tUEeELhXbwTJWq+aGpRvHnE7XNaUquMFhMkj8UFX5rvNQD2zHlQ")
                .SetVersion("5.48.4");

            manifest
                .DefineStyle("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.css", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/codemirror.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.48.4/codemirror.css")
                .SetCdnIntegrity("sha384-HQt/yJNY7UN1RAEuaGcFhEV+sZryJ/GL/PmAJVpXnnIt629CWG6kJOTO19nsqw+L", "sha384-W+l0/wPg6VnknJKJ7Zt/4ksAi7+mEa5foJj4Q103tmWYbcmjLQl5omQo4QxEHg9k")
                .SetVersion("5.48.4");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Styles/font-awesome.min.css", "~/OrchardCore.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.min.css", "~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.12.0/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.12.0/css/all.css")
                .SetCdnIntegrity("sha384-REHJTs1r2ErKBuJB0fCK99gCYsVjwxHrSU0N7I1zl9vZbggVJXRMsv/sLlOAGb4M", "sha384-FozkkIZqRmr4BfooWVG/w7UbmmhssZ8jGI0fImoQjxxMCR49K3wuhkAl4L3Pm8Jr")
                .SetVersion("5.12.0");

            manifest
                .DefineScript("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.12.0/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.12.0/js/all.js")
                .SetCdnIntegrity("sha384-S+e0w/GqyDFzOU88KBBRbedIB4IMF55OzWmROqS6nlDcXlEaV8PcFi4DHZYfDk4Y", "sha384-4yJXyYcQJQynz08thMlmIdGJtc1pH25IWEUxeXk25FYh7YxDQqu1NCAbhCC7zip6")
                .SetVersion("5.12.0");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.12.0/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.12.0/js/v4-shims.js")
                .SetCdnIntegrity("sha384-NNQFVmb048Dmy0GH4ex0kGVcjxaPeY6JA1+ImM4Lsfr4HOD5CNMvCSoIfjeJJWX9", "sha384-GRL+whSFS4qAoUG6xDB/5buVvCJ9ZN9jms+GfJYWRNUm0eq2/KRYX9qRJcwxGo+Y")
                .SetVersion("5.12.0");

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
