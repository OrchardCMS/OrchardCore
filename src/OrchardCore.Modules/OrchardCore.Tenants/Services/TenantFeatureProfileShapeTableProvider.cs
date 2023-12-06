using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services;

[Feature("OrchardCore.Tenants.FeatureProfiles")]
public class TenantFeatureProfileShapeTableProvider : IShapeTableProvider
{
    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe("TenantActionTags")
               .OnDisplaying(async displaying =>
               {
                   if (displaying.Shape.TryGetProperty("ShellSettingsEntry", out ShellSettingsEntry entry))
                   {
                       await displaying.Shape.AddAsync(new ShapeViewModel<ShellSettingsEntry>("ProfileFeatureTags", entry), "10");
                   }
               });
    }
}
