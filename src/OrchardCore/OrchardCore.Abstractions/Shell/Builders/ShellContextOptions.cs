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
    }
}
