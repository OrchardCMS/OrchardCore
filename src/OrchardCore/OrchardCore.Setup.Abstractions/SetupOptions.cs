namespace OrchardCore.Setup.Services
{
    /// <summary>
    /// The setup options.
    /// </summary>
    public class SetupOptions
    {
        /// <summary>
        /// The timeout in milliseconds to acquire a distributed setup lock.
        /// </summary>
        public int SetupLockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the distributed setup lock.
        /// </summary>
        public int SetupLockExpiration { get; set; }

        /// <summary>
        /// The distributed lock key which used while setup.
        /// </summary>
        public string SetupLockName { get; set; }
    }
}
