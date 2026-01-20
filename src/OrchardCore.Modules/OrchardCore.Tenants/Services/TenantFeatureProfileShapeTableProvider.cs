using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services;

public class TenantFeatureProfileShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("TenantActionTags")
               .OnDisplaying(async displaying =>
               {
                   if (displaying.Shape.TryGetProperty("ShellSettingsEntry", out ShellSettingsEntry entry))
                   {
                       await displaying.Shape.AddAsync(new ShapeViewModel<ShellSettingsEntry>("ProfileFeatureTags", entry), "10");
                   }
               });

        return ValueTask.CompletedTask;
    }
}
