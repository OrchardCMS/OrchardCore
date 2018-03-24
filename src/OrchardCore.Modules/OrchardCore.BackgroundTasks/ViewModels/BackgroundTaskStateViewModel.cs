namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskStateViewModel
    {
        public string Name { get; set; }
        public BackgroundTaskState State { get; set; }
        public bool NotFound { get; set; }
    }
}
