using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Users.AuditTrail.ResetPassword
{
    public class UserResetPasswordAuditTrailEventConfiguration : IConfigureOptions<AuditTrailOptions>
    {
        public const string User = nameof(User);
        public const string PasswordReset = nameof(PasswordReset);
        public const string PasswordRecovered = nameof(PasswordRecovered);

        public void Configure(AuditTrailOptions options)
        {
            options.For<UserResetPasswordAuditTrailEventConfiguration>(User, S => S["User"])
                .WithEvent(PasswordReset, S => S["Password reset"], S => S["A user successfully reset the password."], true)
                .WithEvent(PasswordRecovered, S => S["Password recovered"], S => S["A user successfully recovered the password."], true);
        }
    }
}
