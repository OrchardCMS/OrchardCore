using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class TwoFactorMethodDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override IDisplayResult Display(TwoFactorMethod model, BuildDisplayContext context)
    {
        if (string.IsNullOrEmpty(model.Provider))
        {
            return null;
        }

        var icon = Initialize<TwoFactorMethod>($"TwoFactorMethod_{model.Provider}_Icon", vm =>
        {
            vm.Provider = model.Provider;
            vm.IsEnabled = model.IsEnabled;
        }).Location("SummaryAdmin", "Icon");

        var content = Initialize<TwoFactorMethod>($"TwoFactorMethod_{model.Provider}_Content", vm =>
        {
            vm.Provider = model.Provider;
            vm.IsEnabled = model.IsEnabled;
        }).Location("SummaryAdmin", "Content");

        var actions = Initialize<TwoFactorMethod>($"TwoFactorMethod_{model.Provider}_Actions", vm =>
        {
            vm.Provider = model.Provider;
            vm.IsEnabled = model.IsEnabled;
        }).Location("SummaryAdmin", "Actions");

        return Combine(icon, content, actions);
    }
}
