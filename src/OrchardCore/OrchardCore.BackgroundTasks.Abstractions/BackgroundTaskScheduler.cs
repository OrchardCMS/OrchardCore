using System;
using NCrontab;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskScheduler : BackgroundTaskState
    {
        public BackgroundTaskScheduler(string tenant, DateTime referenceTime)
        {
            Tenant = tenant;
            ReferenceTime = referenceTime;
        }

        public string Tenant { get; }
        public DateTime ReferenceTime { get; private set; }
        public override DateTime NextStartTime => CrontabSchedule.Parse(Schedule).GetNextOccurrence(ReferenceTime);

        public bool CanRun()
        {
            return Enable && Status == BackgroundTaskStatus.Idle && DateTime.UtcNow >= NextStartTime;
        }

        public void Run()
        {
            Status = BackgroundTaskStatus.Running;
            LastStartTime = ReferenceTime = DateTime.UtcNow;
            StartCount += 1;
        }

        public void Idle()
        {
            if (Status == BackgroundTaskStatus.Running)
            {
                Status = BackgroundTaskStatus.Idle;
                RunningTime = DateTime.UtcNow - LastStartTime;
                TotalTime += RunningTime;
            }
        }

        public void Stop()
        {
            Idle();

            Status = BackgroundTaskStatus.Stopped;
        }

        public void Fault(Exception exception)
        {
            Idle();
            Stop();

            Status = BackgroundTaskStatus.Faulted;
            FaultMessage = exception.Message;
        }
    }
}
