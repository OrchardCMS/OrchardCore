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
        public BackgroundTaskSettings Settings { get; private set; }
        public BackgroundTaskState State { get; private set; }

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
            }

            State.LastExecutionTime = DateTime.UtcNow - State.LastStartTime;
            State.TotalExecutionTime += State.LastExecutionTime;
        }

        public void Fault(Exception exception)
        {
            Idle();
            State.Status = BackgroundTaskStatus.Locked;
            State.FaultMessage = exception.Message;
        }

        public void Command(CommandCode code)
        {
            if (code == CommandCode.Lock)
            {
                State.Status = BackgroundTaskStatus.Locked;
            }
            else if (code == CommandCode.Unlock)
            {
                if (State.Status == BackgroundTaskStatus.Locked)
                {
                    State.Status = BackgroundTaskStatus.Idle;
                }
            }
            else if (code == CommandCode.ResetCount)
            {
                State.StartCount = 0;
                State.LastExecutionTime = new TimeSpan();
                State.TotalExecutionTime = new TimeSpan();
            }
            else if (code == CommandCode.ResetFault)
            {
                State.FaultMessage = String.Empty;
            }
        }

        public BackgroundTaskScheduler Copy()
        {
            return new BackgroundTaskScheduler(Tenant, State.Name, ReferenceTime)
            {
                Settings = Settings.Copy(),
                State = State.Copy()
            };
        }

        public enum CommandCode
        {
            Lock,
            Unlock,
            ResetCount,
            ResetFault
        }
    }
}
