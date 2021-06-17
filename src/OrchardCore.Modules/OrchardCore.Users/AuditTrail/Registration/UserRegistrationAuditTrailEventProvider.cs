using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Users.AuditTrail.Registration
{
    public class UserRegistrationAuditTrailEventProvider : AuditTrailEventProviderBase
    {
        public const string Registered = nameof(Registered);

        public UserRegistrationAuditTrailEventProvider(IStringLocalizer<UserRegistrationAuditTrailEventProvider> stringLocalizer) : base(stringLocalizer)
        { }

        public override void Describe(DescribeContext context)
        {
            if (context.Category != null && context.Category != "User")
            {
                return;
            }

            context.For("User", S["User"])
                .Event(Registered, S["Registered"], S["A user was successfully registered."], true);
        }
    }
}
