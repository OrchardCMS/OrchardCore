using System;

namespace OrchardCore.Users.Events
{
    /// <summary>
    /// Context for password recovery events.
    /// </summary>
    public class PasswordRecoveryContext
    {
        public PasswordRecoveryContext(IUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(User));
        }

        public IUser User { get; }
    }
}
