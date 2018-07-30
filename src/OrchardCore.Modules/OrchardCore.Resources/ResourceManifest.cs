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
                .SetCdn("https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js", "https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.js")
                .SetCdnIntegrity("sha384-nvAa0+6Qg9clwYCGGPpDQLVpLNn0fRaROjHqs13t4Ggj3Ez50XnGQqc/r8MhnRDZ", "sha384-KcyRSlC9FQog/lJsT+QA8AUIFBgnwKM7bxm7/YaX+NTr4D00npYawrX0h+oXI3a2")
                .SetVersion("1.12.4")
                ;

            manifest
                .DefineScript("jQuery")
                .SetCdn("https://ajax.googleapis.com/ajax/libs/jquery/2.2.4/jquery.min.js", "https://ajax.googleapis.com/ajax/libs/jquery/2.2.4/jquery.js")
                .SetCdnIntegrity("sha384-rY/jv8mMhqDabXSo+UCggqKtdmBfd3qC2/KvyTDNQ6PcUJXaxK1tMepoQda4g5vB", "sha384-TlQc6091kl7Au04dPgLW7WK3iey+qO8dAi/LdwxaGBbszLxnizZ4xjPyNrEf+aQt")
                .SetVersion("2.2.4")
                ;

            manifest
                .DefineScript("jQuery")
                .SetCdn("https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js", "https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.js")
                .SetCdnIntegrity("sha384-tsQFqpEReu7ZLhBV2VZlAu7zcOV+rXbYlF2cqB8txI/8aZajjp4Bqd+V6D5IgvKT", "sha384-fJU6sGmyn07b+uD1nMk7/iSb4yvaowcueiQhfVgQuD98rfva8mcr1eSvjchfpMrH")
                .SetVersion("3.3.1")
                ;

            manifest
                .DefineScript("jQuery.slim")
                .SetCdn("https://code.jquery.com/jquery-3.2.1.slim.min.js", "https://code.jquery.com/jquery-3.2.1.slim.js")
                .SetCdnIntegrity("sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN", "sha384-x3iX8AdaBWgXWuBkuyLrg/s5FqjPdFGcfbEuYEFQiQTBs8uX3XX7gkj0BkFhHu7a")
                .SetVersion("3.2.1")
                ;

            manifest
                .DefineScript("jQuery-ui")
                .SetDependencies("jQuery")
                .SetUrl("/OrchardCore.Resources/Scripts/jquery-ui.min.js", "/OrchardCore.Resources/Scripts/jquery-ui.js")
                .SetCdn("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js", "https://code.jquery.com/ui/1.12.1/jquery-ui.js")
                .SetCdnIntegrity("sha384-Dziy8F2VlJQLMShA6FHWNul/veM9bCkRUaLqr199K94ntO5QUrLJBEbYegdSkkqX", "sha384-JPbtLYL10d/Z1crlc6GGGGM3PavCzzoUJ1UxH0bXHOfguWHQ6XAWrIzW+MBGGXe5")
                .SetVersion("1.12.1")
                ;

            manifest
                .DefineStyle("jQuery-ui")
                .SetUrl("/OrchardCore.Resources/Styles/jquery-ui.min.css", "/OrchardCore.Resources/Styles/jquery-ui.css")
                .SetCdn("https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.min.css", "https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css")
                .SetCdnIntegrity("sha384-kcAOn9fN4XSd+TGsNu2OQKSuV5ngOwt7tg73O4EpaD91QXvrfgvf0MR7/2dUjoI6", "sha384-xewr6kSkq3dBbEtB6Z/3oFZmknWn7nHqhLVLrYgzEFRbU/DHSxW7K3B44yWUN60D")
                .SetVersion("1.12.1")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.js")
                .SetCdnIntegrity("sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa", "sha384-OkuKCCwNNAv3fnqHH7lwPY3m5kkvCIUnsHbjdU7sN022wAYaQUfXkqyIZLlL0xQ/")
                .SetVersion("3.3.7")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.css")
                .SetCdnIntegrity("sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u", "sha384-yzOI+AGOH+8sPS29CtL/lEWNFZ+HKVVyYxU0vjId0pMG6xn7UMDo9waPX5ImV0r6")
                .SetVersion("3.3.7")
                ;

            manifest
                .DefineStyle("bootstrap-theme")
                .SetCdn("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.css")
                .SetCdnIntegrity("sha384-rHyoN1iRsVXV4nD0JutlnGaslCJuC7uwjduW9SVrLvRYooPp2bWYgmgJQIXwl/Sp", "sha384-87IgyAZ7ZPkMKNvliJR8lR09U+LMadREF430SkYRoNaFd+l2lhZnI1cXRdWnAZ+3")
                .SetVersion("3.3.7")
                ;

            manifest
                .DefineScript("popper")
                .SetUrl("/OrchardCore.Resources/Scripts/popper.min.js", "/OrchardCore.Resources/Scripts/popper.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.3/umd/popper.min.js", "https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.3/umd/popper.js")
                .SetCdnIntegrity("sha384-ZMP7rVo3mIykV+2+9J3UJ46jBk0WLaUAdn689aCwoqbBJiSnjAK/l8WvCWPIPm49", "sha384-AsJUT69WgSCIrLgOnMRkNjLvBl0aoHtB3vBDnAEOKRfxKMm7gvmSTJUZCefoYWdA")
                .SetVersion("1.14.3")
                ;

            manifest
                .DefineScript("bootstrap")
                .SetDependencies("jQuery", "popper")
                .SetUrl("/OrchardCore.Resources/Scripts/bootstrap.min.js", "/OrchardCore.Resources/Scripts/bootstrap.js")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js", "https://maxcdn.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.js")
                .SetCdnIntegrity("sha384-ChfqqxuZUCnJSK3+MXmPNIyE6ZbWh2IMqE241rYiqJxyMiZ6OW/JmZQ5stwEULTy", "sha384-fyOlGC+soQAvVFysE2KxkXaVKf75M1Zyo6RG7thLEEwD7p6/Cso7G/iV9tPM0C/a")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineStyle("bootstrap")
                .SetUrl("/OrchardCore.Resources/Styles/bootstrap.min.css", "/OrchardCore.Resources/Styles/bootstrap.css")
                .SetCdn("https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css", "https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.css")
                .SetCdnIntegrity("sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO", "sha384-2QMA5oZ3MEXJddkHyZE/e/C1bd30ZUPdzqHrsaHMP3aGDbPA9yh77XDHXC9Imxw+")
                .SetVersion("4.1.3")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetUrl("/OrchardCore.Resources/Styles/font-awesome.min.css", "/OrchardCore.Resources/Styles/font-awesome.css")
                .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css")
                .SetCdnIntegrity("sha384-wvfXpqpZZVQGK6TAh5PVlGOfQNHSoD2xbE+QkPxCAFlNEevoEH3Sl0sibVcOQVnN", "sha384-FckWOBo7yuyMS7In0aXZ0aoVvnInlnFMwCv77x9sZpFgOonQgnBj1uLwenWVtsEj")
                .SetVersion("4.7.0")
                ;

            manifest
                .DefineStyle("font-awesome")
                .SetCdn("https://use.fontawesome.com/releases/v5.2.0/css/all.css")
                .SetCdnIntegrity("sha384-hWVjflwFxL6sNzntih27bfxkr27PmbbK/iSvJ+a4+0owXq79v+lsFkW54bOGbiDQ")
                .SetVersion("5.2.0")
                ;

            manifest
                .DefineScript("font-awesome")
                .SetCdn("https://use.fontawesome.com/releases/v5.2.0/js/all.js")
                .SetCdnIntegrity("sha384-4oV5EgaV02iISL2ban6c/RmotsABqE4yZxZLcYMAdG7FAPsyHYAPpywE9PJo+Khy")
                .SetVersion("5.2.0")
                ;

            manifest
                .DefineScript("font-awesome-v4-shims")
                .SetCdn("https://use.fontawesome.com/releases/v5.2.0/js/v4-shims.js")
                .SetCdnIntegrity("sha384-rn4uxZDX7xwNq5bkqSbpSQ3s4tK9evZrXAO1Gv9WTZK4p1+NFsJvOQmkos19ebn2")
                .SetVersion("5.2.0")
                ;
        }
    }
}
