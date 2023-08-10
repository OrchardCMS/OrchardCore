using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services;

public class TenantShapeTableProvider : IShapeTableProvider
{
    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe("TenantActionTags")
       .OnDisplaying(async displaying =>
       {
           if (displaying.Shape.TryGetProperty("ShellSettingsEntry", out ShellSettingsEntry entry))
           {
               await displaying.Shape.AddAsync(new ShapeViewModel<ShellSettingsEntry>("ManageTenantActionTags", entry), "5");
           }
       });

        builder.Describe("TenantActionButtons")
               .OnDisplaying(async displaying =>
               {
                   if (displaying.Shape.TryGetProperty("ShellSettingsEntry", out ShellSettingsEntry entry))
                   {
                       await displaying.Shape.AddAsync(new ShapeViewModel<ShellSettingsEntry>("ManageTenantActionButtons", entry), "5");
                   }
               });
    }
}
