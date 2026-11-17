using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Drivers;

public sealed class UserNotificationPreferencesPartDisplayDriver : SectionDisplayDriver<User, UserNotificationPreferencesPart>
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;
    private readonly NotificationTypeOptions _notificationTypeOptions;
    private readonly IStringLocalizerFactory _localizerFactory;

    public UserNotificationPreferencesPartDisplayDriver(
        IEnumerable<INotificationMethodProvider> notificationMethodProviders,
        IOptions<NotificationTypeOptions> notificationTypeOptions,
        IStringLocalizerFactory localizerFactory)
    {
        _notificationMethodProviders = notificationMethodProviders;
        _notificationTypeOptions = notificationTypeOptions.Value;
        _localizerFactory = localizerFactory;
    }

    public override Task<IDisplayResult> EditAsync(User user, UserNotificationPreferencesPart part, BuildEditorContext context)
    {
        var result = Initialize<UserNotificationViewModel>("UserNotificationPreferencesPart_Edit", model =>
        {
            var sortedMethods = new List<string>(part.Methods ?? []);
            var optout = part.Optout ?? [];

            // By default the use is opted into all available methods until explicitly optout.
            model.Methods = _notificationMethodProviders.Select(x => x.Method).Except(optout).ToArray();

            model.Optout = optout;

            var availableItems = _notificationMethodProviders
                .Select(provider => new SelectListItem(provider.Name, provider.Method));

            if (sortedMethods.Count > 0)
            {
                model.AvailableMethods = availableItems
                // Sort the methods in the same order they are saved to honor the priority order (i.e., user preferences.)
                .OrderBy(x => sortedMethods.IndexOf(x.Value))
                .ThenBy(x => x.Text);
            }
            else
            {
                model.AvailableMethods = availableItems.OrderBy(x => x.Text);
            }

            model.TypePreferences = _notificationTypeOptions.Types.Values
                .Select(type => new NotificationTypePreferenceViewModel
                {
                    Name = type.Name,
                    DisplayName = _localizerFactory.Localize(type.DisplayName),
                    Description = type.Description is null ? null : _localizerFactory.Localize(type.Description),
                    EnabledMethods = _notificationMethodProviders
                        .Where(provider => NotificationTypePreferences.IsMethodEnabledForType(part, type, provider.Method))
                        .Select(provider => provider.Method)
                        .ToList(),
                })
                .OrderBy(type => type.DisplayName)
                .ToList();

        }).Location("Content:11")
        .RenderWhen(static (notificationMethodProviders) => Task.FromResult(notificationMethodProviders.Any()), _notificationMethodProviders);

        return Task.FromResult<IDisplayResult>(result);
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserNotificationPreferencesPart part, UpdateEditorContext context)
    {
        var model = new UserNotificationViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var sortedMethods = new List<string>(model.SortedMethods ?? []);

        if (sortedMethods.Count > 0)
        {
            // Important to execute this code only when sortedMethods has at least one element to avoid exception.
            // Store all methods in the same order they appear.
            part.Methods = _notificationMethodProviders
                .OrderBy(provider => sortedMethods.IndexOf(provider.Method))
                .ThenBy(provider => provider.Name.ToString())
                .Select(x => x.Method)
                .ToArray();
        }
        else
        {
            part.Methods = _notificationMethodProviders.OrderBy(provider => provider.Name.ToString())
                .Select(x => x.Method)
                .ToArray();
        }

        var selectedMethods = new List<string>(model.Methods ?? []);

        // Store any method that is not selected as an optout.
        part.Optout = _notificationMethodProviders.Where(provider => !selectedMethods.Contains(provider.Method))
            .Select(provider => provider.Method)
            .ToArray();

        // The editor lists every registered type, so a type without a checked method has turned them all off.
        var methods = _notificationMethodProviders.Select(provider => provider.Method).ToArray();

        foreach (var type in _notificationTypeOptions.Types.Values)
        {
            string[] enabledMethods = null;
            model.TypeMethods?.TryGetValue(type.Name, out enabledMethods);

            NotificationTypePreferences.SetEnabledMethods(part, type, methods, enabledMethods);
        }

        return await EditAsync(user, part, context);
    }
}
