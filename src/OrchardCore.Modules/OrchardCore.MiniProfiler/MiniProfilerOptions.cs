namespace OrchardCore.MiniProfiler
{
    public class MiniProfilerOptions
    {
        /// <summary>
        /// Gets or sets whether to allow Mini Profiler on the admin too when the feature is enabled, not just on the
        /// frontend. Defaults to <c>false</c>.
        /// </summary>
        public bool AllowOnAdmin { get; set; }
    }
}
