using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskSettings
    {
        public string Name { get; set; } = String.Empty;
        public bool Enable { get; set; } = true;
        public string Schedule { get; set; } = "* * * * *";
        public string Description { get; set; } = String.Empty;
        public int LockTimeout { get; set; }
        public int LockExpiration { get; set; }

        public bool IsAtomic => LockTimeout > 0 && LockExpiration > 0;
    }
}
