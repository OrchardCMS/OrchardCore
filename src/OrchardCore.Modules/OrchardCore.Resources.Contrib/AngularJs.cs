using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Contrib {
    public class AngularJs : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("AngularJs").SetUrl("/OrchardCore.Resources.Contrib/Scripts/angular.min.js", "/OrchardCore.Resources.Contrib/Scripts/angular.js").SetVersion("1.3.3");
            manifest.DefineScript("AngularJs_Sanitize").SetUrl("/OrchardCore.Resources.Contrib/Scripts/angular-sanitize.min.js", "/OrchardCore.Resources.Contrib/Scripts/angular-sanitize.js").SetVersion("1.3.3").SetDependencies("AngularJs");
            manifest.DefineScript("AngularJs_Resource").SetUrl("/OrchardCore.Resources.Contrib/Scripts/angular-resource.min.js", "/OrchardCore.Resources.Contrib/Scripts/angular-resource.js").SetVersion("1.3.3").SetDependencies("AngularJs");
            manifest.DefineScript("AngularJs_Sortable").SetUrl("/OrchardCore.Resources.Contrib/Scripts/angular-sortable.min.js", "/OrchardCore.Resources.Contrib/Scripts/angular-sortable.js")
                .SetDependencies("AngularJs", "jQueryUI_Sortable");

            manifest.DefineScript("AngularJs_Full").SetDependencies("AngularJs", "AngularJs_Sanitize", "AngularJs_Resource", "AngularJs_Sortable");
        }
    }
}
