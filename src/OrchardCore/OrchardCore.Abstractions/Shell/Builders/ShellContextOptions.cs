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
        /// Should be a low value e.g. 1s as only one instance is intended to remove a given tenant,
        /// and to not block too much the syncing loops of other instances but let them retry again.
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
