using YesSql.Indexes;

namespace OrchardCore.Deployment.Indexes
{
    public class DeploymentPlanIndex : MapIndex
    {
        public long DocumentId { get; set; }

        public string Name { get; set; }
    }

    public class DeploymentPlanIndexProvider : IndexProvider<DeploymentPlan>
    {
        public override void Describe(DescribeContext<DeploymentPlan> context)
        {
            context.For<DeploymentPlanIndex>()
                .Map(deploymentPlan =>
                {
                    return new DeploymentPlanIndex
                    {
                        Name = deploymentPlan.Name
                    };
                });
        }
    }
}
