using System.Collections.Generic;

namespace OrchardCore.Deployment.Core
{
    internal class OrderedDeploymentSourceComparer : IComparer<IOrderedDeploymentSource>
    {
        public int Compare(IOrderedDeploymentSource x, IOrderedDeploymentSource y)
            => x.Order.CompareTo(y.Order);
    }
}
