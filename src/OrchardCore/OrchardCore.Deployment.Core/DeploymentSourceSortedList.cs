using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Core
{
    internal class DeploymentSourceSortedList : IEnumerable<IDeploymentSource>
    {
        private readonly List<IOrderedDeploymentSource> _deploymentSources = new List<IOrderedDeploymentSource>();

        public DeploymentSourceSortedList()
        {

        }

        public DeploymentSourceSortedList(IEnumerable<IDeploymentSource> deploymentSources)
        {
            AddRange(deploymentSources);
        }

        public void Add(IDeploymentSource deploymentSource)
        {
            var orderedDeploymentSource = typeof(IOrderedDeploymentSource).IsAssignableFrom(deploymentSource.GetType())
                ? (IOrderedDeploymentSource)deploymentSource
                : new OrderedDeploymentSource(deploymentSource);

            _deploymentSources.Add(orderedDeploymentSource);

            _deploymentSources.Sort(new OrderedDeploymentSourceComparer());
        }

        public void AddRange(IEnumerable<IDeploymentSource> deploymentSources)
        {
            foreach (var deploymentSource in deploymentSources)
            {
                Add(deploymentSource);
            }
        }

        public IEnumerator<IDeploymentSource> GetEnumerator() => _deploymentSources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class OrderedDeploymentSource : IOrderedDeploymentSource
        {
            private readonly IDeploymentSource _deploymentSource;

            public OrderedDeploymentSource(IDeploymentSource deploymentSource)
            {
                _deploymentSource = deploymentSource ?? throw new ArgumentNullException(nameof(deploymentSource));
            }

            public virtual int Order => 0;

            public async virtual Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                await _deploymentSource.ProcessDeploymentStepAsync(step, result);
            }
        }
    }
}
