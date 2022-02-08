namespace OrchardCore.BackgroundJobs.Models
{
    public class BackgroundJobExecution
    {
        public IBackgroundJob BackgroundJob { get; set; }
        public IBackgroundJobSchedule Schedule { get; set; }
        public BackgroundJobState State { get; set; }
    }
}
