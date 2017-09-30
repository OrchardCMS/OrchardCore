using YesSql.Indexes;

namespace Orchard.Workflows.Models
{
    public class AwaitingActivityIndex : MapIndex
    {
        public string ActivityName { get; set; }
        public bool ActivityStart { get; set; }
    }

    public class AwaitingActivityndexProvider : IndexProvider<Activity>
    {
        public override void Describe(DescribeContext<Activity> context)
        {
            context.For<AwaitingActivityIndex>()
                .Map(awaitingActivity =>
                {
                    return new AwaitingActivityIndex
                    {
                        ActivityName = awaitingActivity.Name,
                        ActivityStart = awaitingActivity.Start
                    };
                });
        }
    }
}
