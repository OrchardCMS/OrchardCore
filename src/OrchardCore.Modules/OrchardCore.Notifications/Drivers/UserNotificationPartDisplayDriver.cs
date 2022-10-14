using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Models;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Drivers;

public class UserNotificationPartDisplayDriver : SectionDisplayDriver<User, UserNotificationPart>
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;

    public UserNotificationPartDisplayDriver(IEnumerable<INotificationMethodProvider> notificationMethodProviders)
    {
        _notificationMethodProviders = notificationMethodProviders;
    }

    public override Task<IDisplayResult> EditAsync(User user, UserNotificationPart part, BuildEditorContext context)
    {
        var result = Initialize<UserNotificationViewModel>("UserNotificationPart_Edit", viewModel =>
        {
            var selectedTypes = new List<string>(part.Methods ?? Array.Empty<string>());
            viewModel.Methods = selectedTypes.ToArray();
            viewModel.Strategy = part.Strategy;

            viewModel.AvailableMethods = _notificationMethodProviders
                .Select(provider => new SelectListItem(provider.Name, provider.Method))
                // Sort the methods in the same order they are saved to honor the priority order (i.e., user preferences.)
                .OrderBy(x => selectedTypes.IndexOf(x.Value));

        }).Location("Content:11");

        return Task.FromResult<IDisplayResult>(result);
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserNotificationPart part, IUpdateModel updater, BuildEditorContext context)
    {
        var vm = new UserNotificationViewModel();

        if (await updater.TryUpdateModelAsync(vm, Prefix))
        {
            part.Methods = vm.Methods ?? Array.Empty<string>();
            part.Strategy = vm.Strategy;
        }

        return await EditAsync(user, part, context);
    }
}
