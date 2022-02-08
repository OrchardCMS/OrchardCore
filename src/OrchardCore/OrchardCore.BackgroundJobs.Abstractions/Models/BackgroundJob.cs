namespace OrchardCore.BackgroundJobs.Models
{
    public abstract class BackgroundJob<T> : IBackgroundJob where T : class, new()
    {
        public string Name => typeof(T).Name;
        public string BackgroundJobId { get; set; }
        public string RepeatCorrelationId { get; set; }
        public string CorrelationId { get; set; }
    }
}
