namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskSettings
    {
        public static BackgroundTaskSettings None = new BackgroundTaskSettings() { Enable = false };

        public bool Enable { get; set; } = true;
        public string Schedule { get; set; } = "* * * * *";
    }
}