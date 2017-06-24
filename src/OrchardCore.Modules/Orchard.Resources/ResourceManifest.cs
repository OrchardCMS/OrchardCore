using Orchard.ResourceManagement;

namespace Orchard.Resources
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("jQuery")
                .SetCdn("https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js")
                .SetVersion("1.12.4")
                ;

            manifest
                .DefineScript("jQuery")
                .SetCdn("https://ajax.googleapis.com/ajax/libs/jquery/2.2.4/jquery.min.js")
                .SetVersion("2.2.4")
                ;

            manifest
                .DefineScript("jQuery")
                .SetCdn("https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js")
                .SetVersion("3.1.1")
                ;

            manifest
                .DefineScript("jQuery.slim")
                .SetCdn("https://code.jquery.com/jquery-3.1.1.slim.min.js", "https://code.jquery.com/jquery-3.1.1.slim.js")
                .SetCdnIntegrity("sha384-A7FZj7v+d/sdmMqp/nOQwliLvUsJfDHW+k9Omg/a/EheAdgtzNs3hpfag6Ed950n")
                .SetVersion("3.1.1")
                ;

            manifest
                .DefineScript("jquery-ui")
                .SetDependencies("jQuery")
                .SetUrl("/Orchard.Resources/Scripts/jquery-ui.min.js", "/Orchard.Resources/Scripts/jquery-ui.js")
                .SetCdn("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js", "https://code.jquery.com/ui/1.12.1/jquery-ui.js")
                ;
            
            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js")
                .SetCdnIntegrity("sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa")
                .SetVersion("3.3.7")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css")
                .SetCdnIntegrity("sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u")
                .SetVersion("3.3.7")
                ;

            manifest
                .DefineStyle("bootstrap-theme")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css")
                .SetCdnIntegrity("sha384-rHyoN1iRsVXV4nD0JutlnGaslCJuC7uwjduW9SVrLvRYooPp2bWYgmgJQIXwl/Sp")
                .SetVersion("3.3.7")
                ;

            manifest
                .DefineScript("tether")
                .SetUrl("/Orchard.Resources/Scripts/tether.js", "/Orchard.Resources/Scripts/bootstrap.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/tether/1.4.0/js/tether.min.js", "https://cdnjs.cloudflare.com/ajax/libs/tether/1.4.0/js/tether.js")
                .SetVersion("1.4.0")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery", "tether")
                .SetUrl("/Orchard.Resources/Scripts/bootstrap.min.js", "/Orchard.Resources/Scripts/bootstrap.js")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/js/bootstrap.min.js")
                .SetCdnIntegrity("sha384-vBWWzlZJ8ea9aCX4pEW3rVHjgjt7zpkNpZk+02D9phzyeVkE+jo0ieGizqPLForn")
                .SetVersion("4.0.0")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css")
                .SetCdnIntegrity("sha384-rwoIResjU2yc3z8GV/NPeZWAv56rSmLldC3R/AZzGRnGxQQKnKkoFVhFQhNUwEyJ")
                .SetVersion("4.0.0")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("/Orchard.Resources/Styles/font-awesome.min.css", "/Orchard.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN")
                .SetVersion("4.7.0")
                ;
        }
    }
}
