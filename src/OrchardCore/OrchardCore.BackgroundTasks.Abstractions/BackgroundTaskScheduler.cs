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
        public BackgroundTaskSettings Settings { get; set; }
        public BackgroundTaskState State { get; set; }

        public bool CanRun()
        {
            State.NextStartTime = CrontabSchedule.Parse(Settings.Schedule).GetNextOccurrence(ReferenceTime);

            if (DateTime.UtcNow >= State.NextStartTime)
            {
                ReferenceTime = DateTime.UtcNow;
                State.NextStartTime = CrontabSchedule.Parse(Settings.Schedule).GetNextOccurrence(ReferenceTime);
                return Settings.Enable && State.Status == BackgroundTaskStatus.Idle;
            }

            return false;
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
            }

            State.LastExecutionTime = DateTime.UtcNow - State.LastStartTime;
            State.TotalExecutionTime += State.LastExecutionTime;
        }

        public void Fault(Exception exception)
        {
            Idle();
            State.Status = BackgroundTaskStatus.Locked;
            State.FaultMessage = DateTime.UtcNow.ToString() + ' ' + exception.Message;
        }

        public BackgroundTaskScheduler Command(CommandCode code)
        {
            var scheduler = Clone();

            if (code == CommandCode.Lock)
            {
                scheduler.State.Status = BackgroundTaskStatus.Locked;
            }
            else if (code == CommandCode.Unlock)
            {
                if (scheduler.State.Status == BackgroundTaskStatus.Locked)
                {
                    scheduler.State.Status = BackgroundTaskStatus.Idle;
                }
            }
            else if (code == CommandCode.ResetCount)
            {
                scheduler.State.StartCount = 0;
                scheduler.State.LastExecutionTime = new TimeSpan();
                scheduler.State.TotalExecutionTime = new TimeSpan();
            }
            else if (code == CommandCode.ResetFault)
            {
                scheduler.State.FaultMessage = String.Empty;
            }
            else if (code == CommandCode.Disable)
            {
                scheduler.Settings.Enable = false;
            }
            else if (code == CommandCode.Enable)
            {
                scheduler.Settings.Enable = true;
            }

            return scheduler;
        }

        public BackgroundTaskScheduler Clone()
        {
            return new BackgroundTaskScheduler(Tenant, State.Name, ReferenceTime)
            {
                Settings = Settings.Clone(),
                State = State.Clone()
            };
        }

        public enum CommandCode
        {
            Lock,
            Unlock,
            ResetCount,
            ResetFault,
            Disable,
            Enable
        }
    }
}
