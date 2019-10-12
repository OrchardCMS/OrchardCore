using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Core.Services;
using Xunit;

namespace OrchardCore.Tests.Deployment
{
    public class DeploymentSourceOrderTests
    {
        private static readonly IList<string> _executedDeploymentSteps = new List<string>();

        [Fact]
        public async Task ExecuteDeploymentPlan_ShouldRespect_DeployementSourceOrder()
        {
            var deploymentSources = new IDeploymentSource[] { new DeploymentSource1(), new DeploymentSource2() };
            var deploymentManager = new DeploymentManager(deploymentSources, null, null);      

            using (var fileBuilder = new TemporaryFileBuilder())
            {
                var deploymentPlanResult = new DeploymentPlanResult(fileBuilder);
                var deploymentPlan = new DeploymentPlan();

                deploymentPlan.DeploymentSteps.AddRange(new DeploymentStep[] { new DeploymentStep1(), new DeploymentStep2() });
                await deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
            }

            Assert.Equal("Deployment Step 2", _executedDeploymentSteps[0]);
            Assert.Equal("Deployment Step 1", _executedDeploymentSteps[1]);
        }

        public class DeploymentStep1 : DeploymentStep
        {
            public DeploymentStep1()
            {
                Name = "Deployment Step 1";
            }
        }

        public class DeploymentStep2 : DeploymentStep
        {
            public DeploymentStep2()
            {
                Name = "Deployment Step 2";
            }
        }

        public class DeploymentSource1 : DeploymentSourceBase
        {
            public override async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                var deploymentStep = step as DeploymentStep1;

                if (deploymentStep == null)
                {
                    return;
                }

                result.Steps.Add(new JObject(new JProperty("name", "Deployment Step 1")));
                _executedDeploymentSteps.Add("Deployment Step 1");

                await Task.CompletedTask;
            }
        }

        public class DeploymentSource2 : DeploymentSourceBase
        {
            public override int Order { get; } = -10;

            public override async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                var deploymentStep = step as DeploymentStep2;

                if (deploymentStep == null)
                {
                    return;
                }

                result.Steps.Add(new JObject(new JProperty("name", "Deployment Step 2")));
                _executedDeploymentSteps.Add("Deployment Step 2");

                await Task.CompletedTask;
            }
        }
    }
}