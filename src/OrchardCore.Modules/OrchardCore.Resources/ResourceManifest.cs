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
                .SetCdn("https://code.jquery.com/jquery-3.3.1.min.js", "https://code.jquery.com/jquery-3.3.1.js")
                .SetCdnIntegrity("sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8=", "sha256-2Kok7MbOyxpgUVvAk/HJ2jigOSYS2auK4Pfzbm7uH60=")
                .SetVersion("3.3.1")
                ;

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery.slim.min.js", "~/OrchardCore.Resources/Scripts/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.3.1.slim.min.js", "https://code.jquery.com/jquery-3.3.1.slim.js")
                .SetCdnIntegrity("sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo", "sha384-sh6iinGECmk2oNezd0GDVuXqoHrZzF3PTML2nyt/lC61v2p1W7hGll/JkRFCOcMf")
                .SetVersion("3.3.1")
                ;

            manifest
                .DefineScript("jQuery-ui")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/jquery-ui.min.js", "~/OrchardCore.Resources/Scripts/jquery-ui.js")
                .SetCdn("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js", "https://code.jquery.com/ui/1.12.1/jquery-ui.js")
                .SetCdnIntegrity("sha384-Dziy8F2VlJQLMShA6FHWNul/veM9bCkRUaLqr199K94ntO5QUrLJBEbYegdSkkqX", "sha384-JPbtLYL10d/Z1crlc6GGGGM3PavCzzoUJ1UxH0bXHOfguWHQ6XAWrIzW+MBGGXe5")
                .SetVersion("1.12.1")
                ;

            manifest
                .DefineStyle("jQuery-ui")
                .SetUrl("~/OrchardCore.Resources/Styles/jquery-ui.min.css", "~/OrchardCore.Resources/Styles/jquery-ui.css")
                .SetCdn("https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.min.css", "https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css")
                .SetCdnIntegrity("sha384-kcAOn9fN4XSd+TGsNu2OQKSuV5ngOwt7tg73O4EpaD91QXvrfgvf0MR7/2dUjoI6", "sha384-xewr6kSkq3dBbEtB6Z/3oFZmknWn7nHqhLVLrYgzEFRbU/DHSxW7K3B44yWUN60D")
                .SetVersion("1.12.1")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/js/bootstrap.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/js/bootstrap.js")
                .SetCdnIntegrity("sha384-vhJnz1OVIdLktyixHY4Uk3OHEwdQqPppqYR8+5mjsauETgLOcEynD9oPHhhz18Nw", "sha384-it0Suwx+VjMafDIVf5t+ozEbrflmNjEddSX5LstI/Xdw3nv4qP/a4e8K4k5hH6l4")
                .SetVersion("3.4.0")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap.min.css", "https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap.css")
                .SetCdnIntegrity("sha384-PmY9l28YgO4JwMKbTvgaS7XNZJ30MK9FAZjjzXtlqyZCqBY6X6bXIkM++IkyinN+", "sha384-/5bQ8UYbZnrNY3Mfy6zo9QLgIQD/0CximLKk733r8/pQnXn2mgvhvKhcy43gZtJV")
                .SetVersion("3.4.0")
                ;

            manifest
                .DefineStyle("bootstrap-theme")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap-theme.min.css", "https://maxcdn.bootstrapcdn.com/bootstrap/3.4.0/css/bootstrap-theme.css")
                .SetCdnIntegrity("sha384-jzngWsPS6op3fgRCDTESqrEJwRKck+CILhJVO5VvaAZCq8JYf8HsR/HPpBOOPZfR", "sha384-RtiWe5OsslAYZ9AVyorBziI2VQL7E27rzWygBJh7wrZuVPyK5jeQLLytnJIpJqfD")
                .SetVersion("3.4.0")
                ;

            manifest
                .DefineScript("popper")
                .SetUrl("~/OrchardCore.Resources/Scripts/popper.min.js", "~/OrchardCore.Resources/Scripts/popper.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.6/umd/popper.min.js", "https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.6/umd/popper.js")
                .SetCdnIntegrity("sha384-wHAiFfRlMFy6i5SRaxvfOCifBUQy1xHdJ/yoi7FRNXMRBu5WHdZYu1hA6ZOblgut", "sha384-HzqOR2vfXkFlYAX/3YipGekTG6pn/9zeXoTLZZpSdO3w94laYDd5KXyKA22nTfuQ")
                .SetVersion("1.14.6")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.js")
                .SetCdnIntegrity("sha384-ChfqqxuZUCnJSK3+MXmPNIyE6ZbWh2IMqE241rYiqJxyMiZ6OW/JmZQ5stwEULTy", "sha384-fyOlGC+soQAvVFysE2KxkXaVKf75M1Zyo6RG7thLEEwD7p6/Cso7G/iV9tPM0C/a")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("bootstrap-bundle")
                .SetDependencies("jQuery", "popper")
                .SetUrl("~/OrchardCore.Resources/Scripts/bootstrap.bundle.min.js", "~/OrchardCore.Resources/Scripts/bootstrap.bundle.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.bundle.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-pjaaA8dDz/5BgdFUPX6M/9SUZv4d12SUPF0axWc+VRZkx5xU3daN+lYb49+Ax+Tl", "sha384-DWBJ4L0qV7ffH95jHsoooM04DWR2qtntWspYadu41Wx5kw6d0Cs7W+7C2v2bh7vX")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("~/OrchardCore.Resources/Styles/bootstrap.min.css", "~/OrchardCore.Resources/Styles/bootstrap.css")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css", "https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.css")
                .SetCdnIntegrity("sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO", "sha384-2QMA5oZ3MEXJddkHyZE/e/C1bd30ZUPdzqHrsaHMP3aGDbPA9yh77XDHXC9Imxw+")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/codemirror.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.js")
                .SetCdnIntegrity("sha384-1WlqTuBkhlft5hld74c3aAcO43Mp2uFKAl/z/6tYuEF0kDEnQRWnSIExi+EApxkW", "sha384-x1QKAzaJ+REY7xvp6SmcWnnyQdLJJaudAcV2KGSzDytetEOxiaYyaZ5PFLzBuvwR")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode.min.js", "~/OrchardCore.Resources/Scripts/codemirror/codemirror.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.js")
                .SetCdnIntegrity("sha384-1WlqTuBkhlft5hld74c3aAcO43Mp2uFKAl/z/6tYuEF0kDEnQRWnSIExi+EApxkW", "sha384-x1QKAzaJ+REY7xvp6SmcWnnyQdLJJaudAcV2KGSzDytetEOxiaYyaZ5PFLzBuvwR")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/multiplex.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/multiplex.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.min.js", "~/OrchardCore.Resources/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/simple.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/simple.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/javascript/javascript.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/javascript/javascript.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/sql/sql.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/sql/sql.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.min.js", "~/OrchardCore.Resources/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/xml/xml.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/xml/xml.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineStyle("codemirror")
                .SetUrl("~/OrchardCore.Resources/Styles/codemirror/codemirror.min.css", "~/OrchardCore.Resources/Styles/codemirror/codemirror.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.css")
                .SetCdnIntegrity("sha384-T6md2jYuokZmxpt4u/OxutZZs2NFnA/5oVdjrDkapBl/HHH3NfxhUMbFxEv5NTlh", "sha384-rTt9i9SnVCkukyC4WSJmDVMachnmXt3NchukWtR1miRFWpcgnyeOFxq2FBzsKltl")
                .SetVersion("5.42.0")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("~/OrchardCore.Resources/Styles/font-awesome.min.css", "~/OrchardCore.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetCdn("https://use.fontawesome.com/releases/v5.5.0/css/all.css")
                .SetCdnIntegrity("sha384-B4dIYHKNBt8Bc12p+WXckhzcICo0wtJAoU8YZTY5qE0Id1GSseTk6S+L3BlXeVIU")
                .SetVersion("5.5.0")
                ;

            manifest
                .DefineScript("font-awesome")
                .SetCdn("https://use.fontawesome.com/releases/v5.5.0/js/all.js")
                .SetCdnIntegrity("sha384-GqVMZRt5Gn7tB9D9q7ONtcp4gtHIUEW/yG7h98J7IpE3kpi+srfFyyB/04OV6pG0")
                .SetVersion("5.5.0")
                ;

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetCdn("https://use.fontawesome.com/releases/v5.5.0/js/v4-shims.js")
                .SetCdnIntegrity("sha384-vBDTb50BKnwbvJZ5ZC5dsGJNQydTI7ZoAjCeJkdta6nSewwGXCnppKI5lrIQX4Qu")
                .SetVersion("5.5.0")
                ;
        }
    }
}
