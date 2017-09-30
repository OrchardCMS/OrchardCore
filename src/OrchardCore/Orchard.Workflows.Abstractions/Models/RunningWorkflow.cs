using System.Collections.Generic;

namespace Orchard.Workflows.Models
{
    public class RunningWorkflow
    {
        public int Id { get; set; }
        public string State { get; set; }
        public string ContentItemId { get; set; }
        public IList<AwaitingActivity> AwaitingActivities { get; } = new List<AwaitingActivity>();
    }
}
