using System.Collections.Generic;

namespace OrchardCore.Deployment.Core
{
    internal class DeploymentSourceComparer : IComparer<IDeploymentSource>
    {
        public int Compare(IDeploymentSource x, IDeploymentSource y)
        {
            int xOrder = 0, yOrder = 0;

            if (x is IOrderedDeploymentSource xOrdered)
            {
                xOrder = xOrdered.Order;
            }

            if (y is IOrderedDeploymentSource yOrdered)
            {
                yOrder = yOrdered.Order;
            }

            return xOrder.CompareTo(yOrder);
        }
    }
}
