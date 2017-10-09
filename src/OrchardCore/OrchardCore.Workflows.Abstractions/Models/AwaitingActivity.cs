namespace OrchardCore.Workflows.Models
{
    public class AwaitingActivity
    {
        public static AwaitingActivity FromActivity(Activity activity)
        {
            return new AwaitingActivity
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
