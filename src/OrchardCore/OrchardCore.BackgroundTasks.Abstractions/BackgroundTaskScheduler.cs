using System;
using NCrontab;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskScheduler : BackgroundTaskState
    {
        public BackgroundTaskScheduler(string tenant, string name, DateTime referenceTime)
        {
            Name = name;
            Tenant = tenant;
            ReferenceTime = referenceTime;
        }

        public string Tenant { get; }
        public DateTime ReferenceTime { get; private set; }
        public override DateTime NextStartTime => CrontabSchedule.Parse(Schedule).GetNextOccurrence(ReferenceTime);

        public BackgroundTaskState CloneState()
        {
            return new BackgroundTaskState()
            {
                Name = Name,
                Enable = Enable,
                Schedule = Schedule,
                LastStartTime = LastStartTime,
                NextStartTime = NextStartTime,
                LastExecutionTime = LastExecutionTime,
                TotalExecutionTime = TotalExecutionTime,
                StartCount = StartCount,
                FaultMessage = FaultMessage,
                Status = Status
            };
        }

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
                LastExecutionTime = DateTime.UtcNow - LastStartTime;
                TotalExecutionTime += LastExecutionTime;
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

        public void Reset()
        {
            StartCount = 0;
            LastExecutionTime = new TimeSpan();
            TotalExecutionTime = new TimeSpan();

            if (Status == BackgroundTaskStatus.Running)
            {
                Status = BackgroundTaskStatus.Idle;
            }
        }
    }
}
