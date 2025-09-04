using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services;

[RequireFeatures("OrchardCore.Features")]
public class TenantFeatureShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("TenantActionButtons")
               .OnDisplaying(async displaying =>
               {
                   if (displaying.Shape.TryGetProperty("ShellSettingsEntry", out ShellSettingsEntry entry))
                   {
                       await displaying.Shape.AddAsync(new ShapeViewModel<ShellSettingsEntry>("ManageFeaturesActionButton", entry), "10");
                   }
               });

        return ValueTask.CompletedTask;
    }
}
