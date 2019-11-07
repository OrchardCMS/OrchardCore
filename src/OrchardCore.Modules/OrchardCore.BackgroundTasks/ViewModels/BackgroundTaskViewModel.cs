namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskViewModel : BackgroundTaskSettings
    {
        public string DefaultSchedule { get; set; } = "* * * * *";
    }
}
