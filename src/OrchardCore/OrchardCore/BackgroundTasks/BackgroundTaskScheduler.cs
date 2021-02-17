using System;
using System.Threading.Tasks;
using NCrontab;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskScheduler
    {
        private ILocalClock _localClock;

        public BackgroundTaskScheduler(string tenant, string name, DateTime referenceTime, ILocalClock localClock)
        {
            Name = name;
            Tenant = tenant;
            ReferenceTime = referenceTime;
            Settings = new BackgroundTaskSettings() { Name = name };
            State = new BackgroundTaskState() { Name = name };
            _localClock = localClock;
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
            var referenceTimeLocal = _localClock.ConvertToLocalAsync(ReferenceTime).Result.DateTime;
            var nextStartTime = CrontabSchedule.Parse(Settings.Schedule).GetNextOccurrence(referenceTimeLocal);
            var nowLocal = _localClock.LocalNowAsync.Result.DateTime;

            if (nowLocal >= nextStartTime)
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
