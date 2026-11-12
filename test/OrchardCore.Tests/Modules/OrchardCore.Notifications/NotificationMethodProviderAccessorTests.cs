using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Infrastructure;
using OrchardCore.Localization;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Notifications;

public class NotificationMethodProviderAccessorTests
{
    [Fact]
    public async Task GetProvidersAsync_MessageWithoutType_ReturnsEveryMethodTheUserKeeps()
    {
        var accessor = CreateAccessor();
        var user = CreateUser(new UserNotificationPreferencesPart { Optout = ["SMS"] });

        var providers = await accessor.GetProvidersAsync(user, new NotificationMessage { Subject = "Hi" });

        Assert.Equal(["Email"], providers.Select(p => p.Method));
    }

    [Fact]
    public async Task GetProvidersAsync_TypeWithDisabledMethod_SkipsItByDefault()
    {
        var accessor = CreateAccessor();
        var user = CreateUser(null);

        var providers = await accessor.GetProvidersAsync(user, new NotificationMessage { Subject = "Hi", Type = "mention" });

        Assert.Equal(["Email"], providers.Select(p => p.Method));
    }

    [Fact]
    public async Task GetProvidersAsync_UserChoiceForType_OverridesDefaults()
    {
        var accessor = CreateAccessor();
        var user = CreateUser(new UserNotificationPreferencesPart
        {
            Types = new() { ["Mention"] = new NotificationTypePreference { Optin = ["SMS"], Optout = ["Email"] } },
        });

        var providers = await accessor.GetProvidersAsync(user, new NotificationMessage { Subject = "Hi", Type = "Mention" });

        Assert.Equal(["SMS"], providers.Select(p => p.Method));
    }

    [Fact]
    public async Task GetProvidersAsync_UnregisteredType_UsesTheMethodsOfTheUser()
    {
        var accessor = CreateAccessor();
        var user = CreateUser(null);

        var providers = await accessor.GetProvidersAsync(user, new NotificationMessage { Subject = "Hi", Type = "Unknown" });

        Assert.Equal(["Email", "SMS"], providers.Select(p => p.Method));
    }

    private static NotificationMethodProviderAccessor CreateAccessor()
    {
        var options = new NotificationTypeOptions();
        options.Add("Mention", LocalizationSource.Create("Mentions", typeof(NotificationMethodProviderAccessorTests)), type => type.DisabledMethods.Add("SMS"));

        return new NotificationMethodProviderAccessor(
            [new FakeProvider("Email"), new FakeProvider("SMS")],
            Options.Create(options));
    }

    private static User CreateUser(UserNotificationPreferencesPart part)
    {
        var user = new User { UserId = "user1", UserName = "user1" };

        if (part != null)
        {
            user.Put(part);
        }

        return user;
    }

    private sealed class FakeProvider : INotificationMethodProvider
    {
        public FakeProvider(string method)
        {
            Method = method;
        }

        public string Method { get; }

        public LocalizedString Name => new(Method, Method);

        public Task<Result> SendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}
