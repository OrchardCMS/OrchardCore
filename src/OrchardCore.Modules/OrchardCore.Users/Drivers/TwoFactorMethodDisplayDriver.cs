using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class TwoFactorMethodDisplayDriver : DisplayDriver<TwoFactorMethod>
{
    public override Task<IDisplayResult> DisplayAsync(TwoFactorMethod model, BuildDisplayContext context)
    {
        if (String.IsNullOrEmpty(model.Provider))
        {
            return Task.FromResult<IDisplayResult>(null);
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

        return Task.FromResult<IDisplayResult>(Combine(icon, content, actions));
    }
}
