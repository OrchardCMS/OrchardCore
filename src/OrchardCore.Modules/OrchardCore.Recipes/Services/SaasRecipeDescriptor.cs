using OrchardCore.Environment.Shell;

namespace OrchardCore.Recipes.Services;

public sealed class SaasRecipeDescriptor : JsonRecipeDescriptor
{
    public override string Name => "SaaS";
    public override string DisplayName => "Software as a Service";
    public override string Description => "Provides default SaaS features like managing multiple websites.";
    public override string Author => "The Orchard Core Team";
    public override string WebSite => "https://orchardcore.net";
    public override string Version => "1.0.0";
    public override bool IsSetupRecipe => true;

    public override string[] Categories => ["default"];
    public override string[] Tags => ["developer", "default"];

    public override bool IsAvailable(ShellSettings shellSettings)
    {
        return shellSettings.IsDefaultShell();
    }

    protected override string Json =>
        """
        {
          "name": "SaaS",
          "displayName": "Software as a Service",
          "description": "Provides default SaaS features like managing multiple websites.",
          "author": "The Orchard Core Team",
          "website": "https://orchardcore.net",
          "version": "1.0.0",
          "issetuprecipe": true,
          "categories": [ "default" ],
          "tags": [ "developer", "default" ],
          "steps": [
            {
              "name": "feature",
              "enable": [
                "OrchardCore.Admin",
                "OrchardCore.Diagnostics",
                "OrchardCore.DynamicCache",
                "OrchardCore.HomeRoute",
                "OrchardCore.Localization",
                "OrchardCore.Features",
                "OrchardCore.Navigation",
                "OrchardCore.Recipes",
                "OrchardCore.Resources",
                "OrchardCore.Roles",
                "OrchardCore.Security",
                "OrchardCore.Settings",
                "OrchardCore.Tenants",
                "OrchardCore.Themes",
                "OrchardCore.Users",
                "TheTheme",
                "TheAdmin",
                "SafeMode"
              ]
            },
            {
              "name": "themes",
              "admin": "TheAdmin",
              "site": "TheTheme"
            },
            {
              "name": "settings",
              "HomeRoute": {
                "Action": "Index",
                "Controller": "Home",
                "Area": "TheTheme"
              }
            }
          ]
        }
        """;
}
