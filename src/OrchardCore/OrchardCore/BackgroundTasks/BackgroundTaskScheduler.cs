using System;
using NCrontab;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskScheduler
    {
        public BackgroundTaskScheduler(string tenant, string name, DateTime referenceTime)
        {
            Name = name;
            Tenant = tenant;
            ReferenceTime = referenceTime;
            Settings = new BackgroundTaskSettings() { Name = name };
            State = new BackgroundTaskState() { Name = name };
        }

        public string Name { get; }
        public string Tenant { get; }
        public DateTime ReferenceTime { get; set; }
        public BackgroundTaskSettings Settings { get; set; }
        public BackgroundTaskState State { get; set; }
        public bool Released { get; set; }
        public bool Updated { get; set; }

        public bool CanRun()
        {
            var nextStartTime = CrontabSchedule.Parse(Settings.Schedule).GetNextOccurrence(ReferenceTime);

            if (DateTime.UtcNow >= nextStartTime)
            {
                if (Settings.Enable && !Released && Updated)
                {
                    return true;
                }

                ReferenceTime = DateTime.UtcNow;
            }

            return false;
        }

        public void Run()
        {
            State.LastStartTime = ReferenceTime = DateTime.UtcNow;
        }
    }
}
