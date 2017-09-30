using YesSql.Indexes;

namespace Orchard.Workflows.Models
{
    public class ActivityIndex : MapIndex
    {
        public string Name { get; set; }
        public bool DefinitionEnabled { get; set; }
        public bool Start { get; set; }
    }

    public class ActivityIndexProvider : IndexProvider<Activity>
    {
        public override void Describe(DescribeContext<Activity> context)
        {
            context.For<ActivityIndex>()
                .Map(activity =>
                {
                    return new ActivityIndex
                    {
                        Name = activity.Name,
                        DefinitionEnabled = activity.Definition.Enabled,
                        Start = activity.Start
                    };
                });
        }
    }
}
