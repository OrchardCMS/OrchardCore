namespace OrchardCore.Media
{
    public enum CacheConfiguration
    {
        /// <summary>
        /// Use the default physical file system cache.
        /// </summary>
        Physical = 0,

        /// <summary>
        /// Do not cache resized media assets on the file system.
        /// </summary>
        None = 1
    }
}
