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
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/jquery.min.js", "https://oss.weloveshare.com/Resource/Scripts/jquery.js")
                .SetCdn("https://code.jquery.com/jquery-3.3.1.min.js", "https://code.jquery.com/jquery-3.3.1.js")
                .SetCdnIntegrity("sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8=", "sha256-2Kok7MbOyxpgUVvAk/HJ2jigOSYS2auK4Pfzbm7uH60=")
                .SetVersion("3.3.1")
                ;

            manifest
                .DefineScript("jQuery.slim")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/jquery.slim.min.js", "https://oss.weloveshare.com/Resource/Scripts/jquery.slim.js")
                .SetCdn("https://code.jquery.com/jquery-3.3.1.slim.min.js", "https://code.jquery.com/jquery-3.3.1.slim.js")
                .SetCdnIntegrity("sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo", "sha384-sh6iinGECmk2oNezd0GDVuXqoHrZzF3PTML2nyt/lC61v2p1W7hGll/JkRFCOcMf")
                .SetVersion("3.3.1")
                ;

            manifest
                .DefineScript("jQuery-ui")
                .SetDependencies("jQuery")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/jquery-ui.min.js", "https://oss.weloveshare.com/Resource/Scripts/jquery-ui.js")
                .SetCdn("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js", "https://code.jquery.com/ui/1.12.1/jquery-ui.js")
                .SetCdnIntegrity("sha384-Dziy8F2VlJQLMShA6FHWNul/veM9bCkRUaLqr199K94ntO5QUrLJBEbYegdSkkqX", "sha384-JPbtLYL10d/Z1crlc6GGGGM3PavCzzoUJ1UxH0bXHOfguWHQ6XAWrIzW+MBGGXe5")
                .SetVersion("1.12.1")
                ;

            manifest
                .DefineStyle("jQuery-ui")
                .SetUrl("https://oss.weloveshare.com/Resource/Styles/jquery-ui.min.css", "https://oss.weloveshare.com/Resource/Styles/jquery-ui.css")
                .SetCdn("https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.min.css", "https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css")
                .SetCdnIntegrity("sha384-kcAOn9fN4XSd+TGsNu2OQKSuV5ngOwt7tg73O4EpaD91QXvrfgvf0MR7/2dUjoI6", "sha384-xewr6kSkq3dBbEtB6Z/3oFZmknWn7nHqhLVLrYgzEFRbU/DHSxW7K3B44yWUN60D")
                .SetVersion("1.12.1")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetCdn("https://oss.weloveshare.com/Resource/Scripts/bootstrap.min.js", "https://oss.weloveshare.com/Resource/Scripts/bootstrap.js")
                .SetCdnIntegrity("sha384-vhJnz1OVIdLktyixHY4Uk3OHEwdQqPppqYR8+5mjsauETgLOcEynD9oPHhhz18Nw", "sha384-it0Suwx+VjMafDIVf5t+ozEbrflmNjEddSX5LstI/Xdw3nv4qP/a4e8K4k5hH6l4")
                .SetVersion("3.4.0")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetCdn("https://oss.weloveshare.com/Resource/Styles/bootstrap.min.css", "https://oss.weloveshare.com/Resource/Styles/bootstrap.css")
                .SetCdnIntegrity("sha384-PmY9l28YgO4JwMKbTvgaS7XNZJ30MK9FAZjjzXtlqyZCqBY6X6bXIkM++IkyinN+", "sha384-/5bQ8UYbZnrNY3Mfy6zo9QLgIQD/0CximLKk733r8/pQnXn2mgvhvKhcy43gZtJV")
                .SetVersion("3.4.0")
                ;

            manifest
                .DefineStyle("bootstrap-theme")
                .SetCdn("https://oss.weloveshare.com/Resource/Styles/bootstrap-theme.min.css", "https://oss.weloveshare.com/Resource/Styles/bootstrap-theme.css")
                .SetCdnIntegrity("sha384-jzngWsPS6op3fgRCDTESqrEJwRKck+CILhJVO5VvaAZCq8JYf8HsR/HPpBOOPZfR", "sha384-RtiWe5OsslAYZ9AVyorBziI2VQL7E27rzWygBJh7wrZuVPyK5jeQLLytnJIpJqfD")
                .SetVersion("3.4.0")
                ;

            manifest
                .DefineScript("popper")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/popper.min.js", "https://oss.weloveshare.com/Resource/Scripts/popper.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.6/umd/popper.min.js", "https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.6/umd/popper.js")
                .SetCdnIntegrity("sha384-wHAiFfRlMFy6i5SRaxvfOCifBUQy1xHdJ/yoi7FRNXMRBu5WHdZYu1hA6ZOblgut", "sha384-HzqOR2vfXkFlYAX/3YipGekTG6pn/9zeXoTLZZpSdO3w94laYDd5KXyKA22nTfuQ")
                .SetVersion("1.14.6")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/bootstrap-4.1.3.min.js", "https://oss.weloveshare.com/Resource/Scripts/bootstrap-4.1.3.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.js")
                .SetCdnIntegrity("sha384-ChfqqxuZUCnJSK3+MXmPNIyE6ZbWh2IMqE241rYiqJxyMiZ6OW/JmZQ5stwEULTy", "sha384-fyOlGC+soQAvVFysE2KxkXaVKf75M1Zyo6RG7thLEEwD7p6/Cso7G/iV9tPM0C/a")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("bootstrap-bundle")
                .SetDependencies("jQuery", "popper")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/bootstrap.bundle.min.js", "https://oss.weloveshare.com/Resource/Scripts/bootstrap.bundle.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.bundle.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-pjaaA8dDz/5BgdFUPX6M/9SUZv4d12SUPF0axWc+VRZkx5xU3daN+lYb49+Ax+Tl", "sha384-DWBJ4L0qV7ffH95jHsoooM04DWR2qtntWspYadu41Wx5kw6d0Cs7W+7C2v2bh7vX")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("https://oss.weloveshare.com/Resource/Styles/bootstrap-4.1.3.min.css", "https://oss.weloveshare.com/Resource/Styles/bootstrap-4.1.3.css")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css", "https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.css")
                .SetCdnIntegrity("sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO", "sha384-2QMA5oZ3MEXJddkHyZE/e/C1bd30ZUPdzqHrsaHMP3aGDbPA9yh77XDHXC9Imxw+")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/codemirror.min.js", "https://oss.weloveshare.com/Resource/Scripts/codemirror/codemirror.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.js")
                .SetCdnIntegrity("sha384-1WlqTuBkhlft5hld74c3aAcO43Mp2uFKAl/z/6tYuEF0kDEnQRWnSIExi+EApxkW", "sha384-x1QKAzaJ+REY7xvp6SmcWnnyQdLJJaudAcV2KGSzDytetEOxiaYyaZ5PFLzBuvwR")
                .SetVersion("4.1.3")
                ;


            manifest
                .DefineScript("codemirror-addon-mode-multiplex")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/addon/mode/multiplex.min.js", "https://oss.weloveshare.com/Resource/Scripts/codemirror/addon/mode/multiplex.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/multiplex.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/multiplex.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-addon-mode-simple")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/addon/mode/simple.min.js", "https://oss.weloveshare.com/Resource/Scripts/codemirror/addon/mode/simple.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/simple.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/addon/mode/simple.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-mode-javascript")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/mode/javascript/javascript.min.js", "https://oss.weloveshare.com/Resource/Scripts/codemirror/mode/javascript/javascript.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/javascript/javascript.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/javascript/javascript.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-mode-sql")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/mode/sql/sql.min.js", "https://oss.weloveshare.com/Resource/Scripts/codemirror/mode/sql/sql.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/sql/sql.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/sql/sql.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineScript("codemirror-mode-xml")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/mode/xml/xml.min.js", "https://oss.weloveshare.com/Resource/Scripts/codemirror/mode/xml/xml.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/xml/xml.min.js", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/mode/xml/xml.js")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineStyle("codemirror")
                .SetUrl("https://oss.weloveshare.com/Resource/Scripts/codemirror/codemirror.min.css", "https://oss.weloveshare.com/Resource/Scripts/codemirror/codemirror.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.min.css", "https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.42.0/codemirror.css")
                .SetCdnIntegrity("sha384-T6md2jYuokZmxpt4u/OxutZZs2NFnA/5oVdjrDkapBl/HHH3NfxhUMbFxEv5NTlh", "sha384-rTt9i9SnVCkukyC4WSJmDVMachnmXt3NchukWtR1miRFWpcgnyeOFxq2FBzsKltl")
                .SetVersion("5.42.0")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("https://oss.weloveshare.com/Resource/Styles/font-awesome.min.css", "https://oss.weloveshare.com/Resource/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetCdn("https://oss.weloveshare.com/Resource/all-5.7.2.min.css","https://oss.weloveshare.com/Resource/all-5.7.2.css")
                .SetCdnIntegrity("sha256-nAmazAk6vS34Xqo0BSrTb+abbtFlgsFK7NKSi6o7Y78=", "sha256-DVK12s61Wqwmj3XI0zZ9MFFmnNH8puF/eRHTB4ftKwk=")
                .SetVersion("5.7.2")
                ;

            manifest
                .DefineScript("font-awesome")
                .SetCdn("https://oss.weloveshare.com/Resource/all-5.7.2.min.js","https://oss.weloveshare.com/Resource/all-5.7.2.js")
                .SetCdnIntegrity("sha256-Oq0ot7xtAl3WqR2277bwtP+iuV2uOTCh03M1ZCjIsJw=", "sha256-3thD9grC33Xa/xFJXfs8ZryCIwIn+LTX/k3r3KxSel0=")
                .SetVersion("5.7.2")
                ;

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetCdn("https://oss.weloveshare.com/Resource/v4-shims-5.7.2.min.js","https://oss.weloveshare.com/Resource/v4-shims-5.7.2.js")
                .SetCdnIntegrity("sha256-Dy8KjLriNkSRrlgRJaVAoXdvxOlz8ico4RVRmZJsxD8=", "sha256-Hr8WbqmgdrcXJGhodaZ1ATNeusCHFbb3GxGVyA32C9E=")
                .SetVersion("5.7.2")
                ;
        }
    }
}
