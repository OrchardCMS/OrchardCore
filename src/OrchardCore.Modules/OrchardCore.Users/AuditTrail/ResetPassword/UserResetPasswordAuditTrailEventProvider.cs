using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Users.AuditTrail.ResetPassword
{
    public class UserResetPasswordAuditTrailEventProvider : AuditTrailEventProviderBase
    {
        public const string PasswordReset = nameof(PasswordReset);
        public const string PasswordRecovered = nameof(PasswordRecovered);

        public UserResetPasswordAuditTrailEventProvider(IStringLocalizer<UserResetPasswordAuditTrailEventProvider> stringLocalizer) : base(stringLocalizer)
        { }

        public override void Describe(DescribeContext context)
        {
            if (context.Category != null && context.Category != "User")
            {
                return;
            }

            context.For("User", S["User"])
                .Event(PasswordReset, S["Password reset"], S["A user successfully reset the password."], true)
                .Event(PasswordRecovered, S["Password recovered"], S["A user successfully recovered the password."], true);
        }
    }
}
