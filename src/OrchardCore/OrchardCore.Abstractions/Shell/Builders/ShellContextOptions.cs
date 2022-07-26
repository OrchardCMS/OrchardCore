namespace OrchardCore.Environment.Shell.Builders
{
    public class ShellContextOptions
    {
        /// <summary>
        /// The timeout in milliseconds to acquire a distributed lock before activating a given shell.
        /// Note: Only used if the current distributed lock implementation is not a local lock.
        /// </summary>
        public int ShellActivateLockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the distributed lock acquired before activating a shell.
        /// Note: Only used if the current distributed lock implementation is not a local lock.
        /// </summary>
        public int ShellActivateLockExpiration { get; set; }

        /// <summary>
        /// The timeout in milliseconds to acquire a distributed lock before removing a given shell.
        /// A low value is recommended because only one instance is intended to drop a given tenant.
        /// This also prevents other instances from waiting too long in their tenant syncing loop.
        /// Note: Only used if the current distributed lock implementation is not a local lock.
        /// </summary>
        public int ShellRemovingLockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the distributed lock acquired before removing a shell.
        /// Note: Only used if the current distributed lock implementation is not a local lock.
        /// </summary>
        public int ShellRemovingLockExpiration { get; set; }
    }
}
