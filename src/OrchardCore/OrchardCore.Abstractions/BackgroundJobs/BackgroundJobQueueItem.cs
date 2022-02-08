using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.BackgroundJobs
{
    public readonly struct BackgroundJobQueueItem
    {
        public BackgroundJobQueueItem(string shellName, IBackgroundJob backgroundJob, Task ready)
        {
            ShellName = shellName;
            BackgroundJob = backgroundJob;
            Ready = ready;
        }

        public string ShellName { get; }
        public IBackgroundJob BackgroundJob { get; }
        public Task Ready { get;  }
    }
}
