using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Users.AuditTrail.Models;

namespace OrchardCore.Users.AuditTrail.Providers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class UserAuditTrailEventProvider : AuditTrailEventProviderBase
    {
        public const string Registered = nameof(Registered);
        public const string LoggedIn = nameof(LoggedIn);
        public const string LogInFailed = nameof(LogInFailed);
        public const string PasswordReset = nameof(PasswordReset);
        public const string PasswordRecovered = nameof(PasswordRecovered);
        public const string Enabled = nameof(Enabled);
        public const string Disabled = nameof(Disabled);
        public const string Created = nameof(Created);
        public const string Deleted = nameof(Deleted);

        public UserAuditTrailEventProvider(IStringLocalizer<UserAuditTrailEventProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override void Describe(DescribeContext context) =>
            context.For<UserAuditTrailEventProvider>("User", S["User"])
                .Event(Registered, S["Registered"], S["A user was successfully registered."], BuildEvent, true)
                .Event(LoggedIn, S["Logged in"], S["A user was successfully logged in."], BuildEvent, true)
                .Event(LogInFailed, S["Login failed"], S["An attempt to login failed due to incorrect credentials."], BuildEvent, true)
                .Event(PasswordReset, S["Password reset"], S["A user successfully reset the password."], BuildEvent, true)
                .Event(PasswordRecovered, S["Password recovered"], S["A user successfully recovered the password."], BuildEvent, true)
                .Event(Enabled, S["Enabled"], S["A user was enabled."], BuildEvent, true)
                .Event(Disabled, S["Disabled"], S["A user was disabled."], BuildEvent, true)
                .Event(Created, S["Created"], S["A user was created."], BuildEvent, true)
                .Event(Deleted, S["Deleted"], S["A user was deleted."], BuildEvent, true);

        private static void BuildEvent(AuditTrailEvent @event, Dictionary<string, object> eventData)
        {
            @event.Put(new AuditTrailUserEvent
            {
                EventName = @event.Name,
                UserName = eventData.Get<string>("UserName"),
                UserId = eventData.Get<string>("UserId")
            });
        }
    }
}
