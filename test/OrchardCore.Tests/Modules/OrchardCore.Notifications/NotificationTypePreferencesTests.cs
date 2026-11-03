using OrchardCore.Localization;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Notifications;

public class NotificationTypePreferencesTests
{
    private static readonly string[] _methods = ["Email", "SMS"];

    [Fact]
    public void IsMethodEnabledForType_NoPreferences_UsesTypeDefaults()
    {
        var type = CreateType(disabled: "SMS");

        Assert.True(NotificationTypePreferences.IsMethodEnabledForType(null, type, "Email"));
        Assert.False(NotificationTypePreferences.IsMethodEnabledForType(null, type, "SMS"));
    }

    [Fact]
    public void SetEnabledMethods_SameAsDefaults_StoresNothing()
    {
        var type = CreateType(disabled: "SMS");
        var part = new UserNotificationPreferencesPart();

        NotificationTypePreferences.SetEnabledMethods(part, type, _methods, ["Email"]);

        Assert.Empty(part.Types);
    }

    [Fact]
    public void SetEnabledMethods_DifferentFromDefaults_StoresOnlyTheChoices()
    {
        var type = CreateType(disabled: "SMS");
        var part = new UserNotificationPreferencesPart();

        NotificationTypePreferences.SetEnabledMethods(part, type, _methods, ["SMS"]);

        Assert.Equal(["SMS"], part.Types["Mention"].Optin);
        Assert.Equal(["Email"], part.Types["Mention"].Optout);
        Assert.True(NotificationTypePreferences.IsMethodEnabledForType(part, type, "SMS"));
        Assert.False(NotificationTypePreferences.IsMethodEnabledForType(part, type, "Email"));
    }

    [Fact]
    public void IsMethodEnabledForType_MethodAddedAfterChoice_UsesTypeDefault()
    {
        var type = CreateType();
        var part = new UserNotificationPreferencesPart();

        NotificationTypePreferences.SetEnabledMethods(part, type, _methods, []);

        // A method that did not exist when the user chose is on, as the type turns it on by default.
        Assert.True(NotificationTypePreferences.IsMethodEnabledForType(part, type, "Push"));
    }

    private static NotificationTypeDescriptor CreateType(params string[] disabled)
    {
        var type = new NotificationTypeDescriptor("Mention", LocalizationSource.Create("Mentions", typeof(NotificationTypePreferencesTests)));

        foreach (var method in disabled)
        {
            type.DisabledMethods.Add(method);
        }

        return type;
    }
}
