namespace OrchardCore.AutoSetup.Options
{
    /// <summary>
    /// The auto setup lock options.
    /// </summary>
    public class LockOptions
    {
        /// <summary>
        /// The timeout in milliseconds to acquire a distributed setup lock.
        /// </summary>
        public int LockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the distributed setup lock.
        /// </summary>
        public int LockExpiration { get; set; }
    }
}
