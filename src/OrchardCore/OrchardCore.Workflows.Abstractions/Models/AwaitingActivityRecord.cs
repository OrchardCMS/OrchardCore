namespace OrchardCore.Workflows.Models
{
    public class AwaitingActivityRecord
    {
        public static AwaitingActivityRecord FromActivity(ActivityRecord activity)
        {
            return new AwaitingActivityRecord
            {
                ActivityId = activity.Id,
                IsStart = activity.IsStart,
                Name = activity.Name
            };
        }

        public int ActivityId { get; set; }
        public bool IsStart { get; set; }
        public string Name { get; set; }
    }
}
