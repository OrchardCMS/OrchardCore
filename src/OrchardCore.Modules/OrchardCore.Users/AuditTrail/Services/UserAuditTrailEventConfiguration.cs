using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Users.AuditTrail.Services
{
    public class UserAuditTrailEventConfiguration : IConfigureOptions<AuditTrailOptions>
    {
        public const string User = nameof(User);
        public const string Registered = nameof(Registered);
        public const string LoggedIn = nameof(LoggedIn);
        public const string LogInFailed = nameof(LogInFailed);
        public const string LogInIsLockedOut = nameof(LogInIsLockedOut);
        public const string Enabled = nameof(Enabled);
        public const string Disabled = nameof(Disabled);
        public const string Created = nameof(Created);
        public const string Updated = nameof(Updated);
        public const string Deleted = nameof(Deleted);

        public void Configure(AuditTrailOptions options)
        {
            options.For<UserAuditTrailEventConfiguration>(User, S => S["User"])
                .WithEvent(LoggedIn, S => S["Logged in"], S => S["A user was successfully logged in."], true)
                .WithEvent(LogInFailed, S => S["Login failed"], S => S["An attempt to login failed."], false) // Intentionally not enabled by default.
                .WithEvent(LogInIsLockedOut, S => S["Login account locked"], S => S["An attempt to login failed because the user is locked out."], true)
                .WithEvent(Enabled, S => S["Enabled"], S => S["A user was enabled."], true)
                .WithEvent(Disabled, S => S["Disabled"], S => S["A user was disabled."], true)
                .WithEvent(Created, S => S["Created"], S => S["A user was created."], true)
                .WithEvent(Updated, S => S["Updated"], S => S["A user was updated."], true)
                .WithEvent(Deleted, S => S["Deleted"], S => S["A user was deleted."], true);
        }
    }
}
