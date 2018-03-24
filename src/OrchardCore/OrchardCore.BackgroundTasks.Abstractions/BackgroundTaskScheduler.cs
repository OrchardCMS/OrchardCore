using System;
using NCrontab;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskScheduler
    {
        public BackgroundTaskScheduler(string tenant, string name, DateTime referenceTime)
        {
            Tenant = tenant;
            ReferenceTime = referenceTime;
            Settings = new BackgroundTaskSettings() { Name = name };
            State = new BackgroundTaskState() { Name = name };
        }

        public string Tenant { get; }
        public DateTime ReferenceTime { get; private set; }
        public BackgroundTaskSettings Settings { get; }
        public BackgroundTaskState State { get; }

        public bool CanRun()
        {
            State.NextStartTime = CrontabSchedule.Parse(Settings.Schedule).GetNextOccurrence(ReferenceTime);
            return Settings.Enable && State.Status == BackgroundTaskStatus.Idle && DateTime.UtcNow >= State.NextStartTime;
        }

        public void Run()
        {
            State.Status = BackgroundTaskStatus.Running;
            State.LastStartTime = ReferenceTime = DateTime.UtcNow;
            State.StartCount += 1;
        }

        public void Idle()
        {
            if (State.Status == BackgroundTaskStatus.Running)
            {
                State.Status = BackgroundTaskStatus.Idle;
                State.LastExecutionTime = DateTime.UtcNow - State.LastStartTime;
                State.TotalExecutionTime += State.LastExecutionTime;
            }
        }

        public void Stop()
        {
            Idle();
            State.Status = BackgroundTaskStatus.Stopped;
        }

        public void Fault(Exception exception)
        {
            Idle();
            Stop();
            State.Status = BackgroundTaskStatus.Faulted;
            State.FaultMessage = exception.Message;
        }

        public void Reset()
        {
            State.StartCount = 0;
            State.LastExecutionTime = new TimeSpan();
            State.TotalExecutionTime = new TimeSpan();

            if (State.Status != BackgroundTaskStatus.Running)
            {
                State.Status = BackgroundTaskStatus.Idle;
            }
        }
    }
}
