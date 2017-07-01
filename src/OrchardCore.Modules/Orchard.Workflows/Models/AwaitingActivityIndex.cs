using YesSql.Indexes;

namespace Orchard.Workflows.Models
{
    public class AwaitingActivityIndex : MapIndex
    {
        public string ActivityName { get; set; }
        public bool ActivityStart { get; set; }
    }

    public class AwaitingActivityndexProvider : IndexProvider<AwaitingActivity>
    {
        public override void Describe(DescribeContext<AwaitingActivity> context)
        {
            context.For<AwaitingActivityIndex>()
                .Map(awaitingActivity =>
                {
                    return new AwaitingActivityIndex
                    {
                        ActivityName = awaitingActivity.Activity.Name,
                        ActivityStart = awaitingActivity.Activity.Start
                    };
                });
        }
    }
}
