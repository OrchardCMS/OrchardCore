namespace OrchardCore.AutoSetup.Options
{
    /// <summary>
    /// The AutoSetup lock options.
    /// </summary>
    public class LockOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockOptions"/> class with Default Lock Option Values.
        /// </summary>
        public LockOptions()
        {
            LockExpiration = 20_000;
            LockTimeout = 20_000;
            LockName = "AUTOSETUP_LOCK";
        }

        /// <summary>
        /// The timeout in milliseconds to acquire a distributed setup lock.
        /// </summary>
        public int LockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the distributed setup lock.
        /// </summary>
        public int LockExpiration { get; set; }

        /// <summary>
        /// The distributed lock key which used while setup.
        /// </summary>
        public string LockName { get; set; }
    }
}
