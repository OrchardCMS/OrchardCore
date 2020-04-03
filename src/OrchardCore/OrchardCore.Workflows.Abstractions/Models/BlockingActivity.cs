namespace OrchardCore.Workflows.Models
{
    public class BlockingActivity
    {
        public static BlockingActivity FromActivity(ActivityRecord activity)
        {
            return new BlockingActivity
            {
                ActivityId = activity.ActivityId,
                IsStart = activity.IsStart,
                Name = activity.Name
            };
        }

        public string ActivityId { get; set; }
        public bool IsStart { get; set; }
        public string Name { get; set; }
    }
}
