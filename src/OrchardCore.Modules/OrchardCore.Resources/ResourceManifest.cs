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
                .SetCdn("https://code.jquery.com/jquery-3.5.1.min.js", "https://code.jquery.com/jquery-3.5.1.js")
                .SetCdnIntegrity("sha384-ZvpUoO/+PpLXR1lu4jmpXWu80pZlYUAfxl5NsBMWOEPSjUn/6Z/hRTt8+pR6L4N2", "sha384-/LjQZzcpTzaYn7qWqRIWYC5l8FWEZ2bIHIz0D73Uzba4pShEcdLdZyZkI4Kv676E")
                .SetVersion("3.5.1");

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.slim.min.js", "~/OrchardCore.Resources/Scripts/jquery.slim.js")
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
                .SetUrl("~/OrchardCore.Resources/Scripts/popper.min.js", "~/OrchardCore.Resources/Scripts/popper.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js", "https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.js")
                .SetCdnIntegrity("sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo", "sha384-EsqqCR7beeX9mWjsrB8ySgz3pKDhWr3OgqnudRtew5RApIIhEN6/qqiPM99Lk9qM")
                .SetVersion("1.16.0");

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery", "popper")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js", "https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.js")
                .SetCdnIntegrity("sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI", "sha384-7emZq+z4THDbp1s8SKlmK0zlENQgT+twJBBAcJCe8c+mastOWEfHflsBcz9t1ste")
                .SetVersion("4.5.0");

            manifest
                .DefineScript("bootstrap-bundle")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.bundle.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.bundle.min.js", "https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-1CmrxMRARb6aLqgBO7yyAxTOQE2AKb9GfXnEo760AUcUmFx3ibVJJAzGytlQcNXd", "sha384-cCFlyGmw6CL62KEUKL7PWDyTOf28usI04ep/5Re2w+M71E1K/sPaE0az/Zj17YG0")
                .SetVersion("4.5.0");

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.min.css", "~/OrchardCore.Resources/Styles/bootstrap.css")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css", "https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.css")
                .SetCdnIntegrity("sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk", "sha384-BHMmCeZEB8FFTwXRrSSWZJd7NXU/Hh4EawgpQO+3MDzE/GMYgbXgHb8ylJcUlBeK")
                .SetVersion("4.5.0");

            manifest
                .DefineStyle("bootstrap-select")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap-select.min.css", "~/OrchardCore.Resources/Styles/bootstrap-select.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.18/dist/css/bootstrap-select.min.css", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.18/dist/css/bootstrap-select.css")
                .SetCdnIntegrity("sha384-dTqTc7d5t+FKhTIaMmda32pZNoXY/Y0ui0hRl5GzDQp4aARfEzbP1jzX6+KRuGKg", "sha384-OlTrhEtwZzUzVXapTUO8s6QryXzpD8mFyNVA8kyAi8KMgfOKSJYvielvExM+dNPR")
                .SetVersion("1.13.18");

            manifest
                .DefineScript("bootstrap-select")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap-select.min.js", "~/OrchardCore.Resources/Scripts/bootstrap-select.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.18/dist/js/bootstrap-select.min.js", "https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.18/dist/js/bootstrap-select.js")
                .SetCdnIntegrity("sha384-x8fxIWvLdZnkRaCvkDXOlxd6UP+qga975FjnbRp6NRpL9jjXjY9TwF9y7z4AccxS", "sha384-6BZTOUHC4e3nWcy5gveLqAu52vwy5TX8zBIvvfZFVDzIjYDgprdXRMK/hsypxdpQ")
                .SetVersion("1.13.18");

            manifest
                .DefineStyle("bootstrap-slider")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap-slider.min.css", "~/OrchardCore.Resources/Styles/bootstrap-slider.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/bootstrap-slider/11.0.2/css/bootstrap-slider.min.css", "https://cdnjs.cloudflare.com/ajax/libs/bootstrap-slider/11.0.2/css/bootstrap-slider.css")
                .SetCdnIntegrity("sha384-Ot7O5p8Ws9qngwQOA1DP7acHuGIfK63cYbVJRYzrrMXhT3syEYhEsg+uqPsPpRhZ", "sha384-x1BbAB1QrM4/ZjT+vJzuI/NdvRo4tINKqg7lTN9jCq0bWrr/nelp9KfroZWd3UJu")
                .SetVersion("11.0.2");

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
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/codemirror.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/codemirror.css")
                .SetCdnIntegrity("sha384-IOz5QFCDEKXQEiYKlSOUsHhcrCpRO2Vn9EcidoFf6olMAZVXnLgcoQclMBoksUOa", "sha384-mVNiRdLEUGBMLPITebcG8Bc5wJW9HT/x0TiXmad0ZMPF9g0CMX2IYLNHqGjCmUsU")
                .SetVersion("5.55.0");

            manifest
                .DefineStyle("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css", "~/OrchardCore.Resources/Styles/codemirror/addon/display/fullscreen.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/display/fullscreen.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/display/fullscreen.css")
                .SetCdnIntegrity("sha384-uuIczW2AGKADJpvg6YiNBJQWE7duDkgQDkndYEsbUGaLm8SPJZzlly6hcEo0aTlW", "sha384-+glu1jsbG+T5ocmkeMvIYh5w07IXKxmJZaCdkNbVfpEr3xi+M0gopFSR/oLKXxio")
                .SetVersion("5.55.0");

            manifest
                .DefineStyle("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.min.css", "~/OrchardCore.Resources/Styles/codemirror/addon/hint/show-hint.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/hint/show-hint.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/hint/show-hint.css")
                .SetCdnIntegrity("sha384-qqTWkykzuDLx4yDYa7bVrwNwBHuqVvklDUMVaU4eezgNUEgGbP8Zv6i3u8OmtuWg", "sha384-ZZbLvEvLoXKrHo3Tkh7W8amMgoHFkDzWe8IAm1ZgxsG5y35H+fJCVMWwr0YBAEGA")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/codemirror.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/codemirror.js")
                .SetCdnIntegrity("sha384-2q2dmT4FdoV5EKP/46gAsS6fVZcNfpND5PXtOQ63Vm9DecdlRpI9K9vrSLgtKrkZ", "sha384-iNxnZH939ze9W2Gf/JE5M7wOgMbx78OwS7zDiphzE9o9t2L/3UzxxhgkHiWsvEnH")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-selection-active-line")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/selection/active-line.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/selection/active-line.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/selection/active-line.min.js")
                .SetCdnIntegrity("sha384-a1C6GgT4/STEs5Df0Ko1uzqijvgtY7MvVv7Z4uuyk+N4llXey69UU0QxXy6L007K", "sha384-kKz13r+qZMgTNgXROGNHQ0/0/J1FtvIvRZ9yjOHo1YLUCd+KF8r9R+su/B+f6C0U")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-display-autorefresh")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/autorefresh.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/display/autorefresh.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/display/autorefresh.js")
                .SetCdnIntegrity("sha384-pn83o6MtS8kicn/sV6AhRaBqXQ5tau8NzA2ovcobkcc1uRFP7D8CMhRx231QwKST", "sha384-5wrQkkCzj5dJInF+DDDYjE1itTGaIxO+TL6gMZ2eZBo9OyWLczkjolFX8kixX/9X")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-display-fullscreen")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/display/fullscreen.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/display/fullscreen.js")
                .SetCdnIntegrity("sha384-QWNlxQIqNTw1WRsFEBoyGSFItEfal64eVEm0CU8KVFq6P5KnKOggFNp9aQgmDA9U", "sha384-kuwHYEheDAp0NlPye2VXyxJ6J54CeVky9tH+e0mn6/gP4PJ50MF4FGKkrGAxIyCW")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-edit-closetag")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/edit/closetag.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/display/fullscreen.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/edit/closetag.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/edit/closetag.js")
                .SetCdnIntegrity("sha384-JqEk/xYh2w3m+IwV7QlyAJI+nLEISeQQzipKvsXonJCTFrAerFYuxnl/UcaYtImK", "sha384-7U8y7U7Dit7biOcvavud1w0ihUHjWoWv8Fp1JmgbDkADBCu2EqQ27uzZ595vlX7g")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/show-hint.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/hint/show-hint.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/hint/show-hint.js")
                .SetCdnIntegrity("sha384-3UITvU6gxHCYZ/Whow/mWEJG/DbpbDqRHcQqJ/miBe86ENtkm+z62llgRoAH+MQY", "sha384-USiVlKYPkFPEbrbUlT3DURUHeO3ogAjo+7SWnL+McPGgYdPV4KZvWewUJWLW38I3")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-hint-sql-hint")
                .SetDependencies("codemirror-addon-hint-show-hint")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/hint/sql-hint.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/hint/sql-hint.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/hint/sql-hint.js")
                .SetCdnIntegrity("sha384-7T1bk5aA9rLYBK31d6goGFhrztC+Vnua7okS4cwlyvbC75jOdAXs87DcTebQIdp6", "sha384-CbJvaXp/cZi3jgxVlTeaH3peT3az2eDoPjzKvYTFKyTOQXEOrHPIhX0TWErwJeBB")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/mode/multiplex.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/mode/multiplex.js")
                .SetCdnIntegrity("sha384-sTw3lU1fIEXG/qNbuuh7lpitFgxSwaReQLtKo8gVCzKz/j3yOk8whDk4NiG5Cnvs", "sha384-xF886YuXgjqY02JjHuoV+B+SZYB4DYq7YUniuin/yCqJdxYpXomTlOfmiSoSNpwQ")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/mode/simple.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/addon/mode/simple.js")
                .SetCdnIntegrity("sha384-DbFZlXwgV4md0LMM7aSVdbUaCldkPh4QXILJ/kbD9ZZiC3gQ+E9SjHG++B3PvCr1", "sha384-ntjFEzI50GYBTbLGaOVgBt97cxp74jfCqMDmZYlGWk8ZZp2leFMJYOp85T3tOeG9")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-mode-css")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/css/css.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/css/css.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/css/css.js")
                .SetCdnIntegrity("sha384-7jWEUNIiKACsg4CC/mH6GAi028ses/AR9YrcDgggjvCNbAMPGzAntlk7wZCcbFVK", "sha384-EOd2XWza2wnlyji1uXvCHhzv8KidkG8oG2DUP104jMguvgGdf62zxdt6l/f8pVp8")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-mode-htmlmixed")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/htmlmixed/htmlmixed.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/htmlmixed/htmlmixed.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/htmlmixed/htmlmixed.js")
                .SetCdnIntegrity("sha384-sbLNpMWktagHnz1dYX5ffxjnU0+k5cGjZTg2QDE64Gi/fZ28b0USo6CZvaEjyKKl", "sha384-fxS517EbIB2hObIbibF9AuQAFnAlwZgIsy2zpeCqpw4yIkJRfNOLSe0JHu7H7ULE")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/javascript/javascript.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/javascript/javascript.js")
                .SetCdnIntegrity("sha384-vkgBOd88TFSLPlWdjQF8Z88Y+rdx89mCNxdRjszoktP9cKGywCYWsVJaGaxPVTXh", "sha384-JZ4MmB/E5TeRrJNZ2sRaBrsgzi0QmpwTjYeiYWJ82LKDWINSGZJ8z5LlOFRmkj72")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/sql/sql.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/sql/sql.js")
                .SetCdnIntegrity("sha384-86IcFNbXZf+LKDhAwqi2Rvj//vHEiIK6AzJ0j2m5OzgIWSOsCcPWzTu1VA61Xo8H", "sha384-D+uYw7olvmCEyME9PHzGRne1Sh8+XTk84/JZinlhDTutXBH45AdThw4uLAAt4vIm")
                .SetVersion("5.55.0");

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/xml/xml.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.55.0/mode/xml/xml.js")
                .SetCdnIntegrity("sha384-27kgmf2cIJ9bclpvyUN4SgnnYnW7Wu0MiGYpm+fmNQhcREFI7mq0C9ehuxYuwqgy", "sha384-z8O1dAcb8dxGYbhmbzbJ41xAUlle9jfZpok46e8s9+Tyu9bKxx742hiNqVJ0D6cL")
                .SetVersion("5.55.0");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Styles/font-awesome.min.css", "~/OrchardCore.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0");

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.min.css", "~/OrchardCore.Resources/Vendor/fontawesome-free/css/all.css")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.1/css/all.min.css", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.1/css/all.css")
                .SetCdnIntegrity("sha384-xxzQGERXS00kBmZW/6qxqJPyxW3UR0BPsL4c8ILaIWXva5kFi7TxkIIaMiKtqV1Q", "sha384-2qrhYpFXjhlg6c2KJIb8Nug1IjoXg4uCzGmD1Qf626SEQ1+xvMTkW6Cay/2E6XdH")
                .SetVersion("5.13.1");

            manifest
                .DefineScript("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/all.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.1/js/all.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.1/js/all.js")
                .SetCdnIntegrity("sha384-heKROmDHlJdBb+n64p+i+wLplNYUZPaZmp2HZ4J6KCqzmd33FJ8QClrOV3IdHZm5", "sha384-mdHom2NzFDmRZygnlVhOWWUhqmfUXe6oAia030X1UNhlPFU/HGAEQDXXw/MKQEBh")
                .SetVersion("5.13.1");

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetUrl("~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.min.js", "~/OrchardCore.Resources/Vendor/fontawesome-free/js/v4-shims.js")
                .SetCdn("https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.1/js/v4-shims.min.js", "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@5.13.1/js/v4-shims.js")
                .SetCdnIntegrity("sha384-CtI1i5e/58ZMUgZkT75wgboNZAbEXBNToPY17SfEmfyKvGuJW7DP418LdXkjI++F", "sha384-mLshlNnOwes7FKsG54w+kpzBeZbYB7RfZLn0fifKjaV0BmdHB4vMrIoZgYLZnV4L")
                .SetVersion("5.13.1");

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
