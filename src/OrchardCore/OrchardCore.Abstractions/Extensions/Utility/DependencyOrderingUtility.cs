using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Extensions.Utility
{
    public static class DependencyOrdering
    {
        private class Node<T>
        {
            public T Item { get; set; }
            public bool Used { get; set; }
        }

        /// <summary>
        /// Linearizes a dependency graph so that items are positioned after their dependencies.
        /// This by using a function which determines if an item has a direct dependency on another.
        /// Then, items are moved up whenever it is possible without breaking the dependency graph.
        /// This by using a function which gives for each item a priority used as an order value.
        /// </summary>
        public static IEnumerable<T> OrderByDependenciesAndPriorities<T>(this IEnumerable<T> items, Func<T, T, bool> hasDependency, Func<T, int> getPriority)
        {
            var nodes = items.Select(i => new Node<T> { Item = i }).ToArray();

            var result = new List<T>();
            foreach (var node in nodes)
            {
                Add(node, result, nodes, hasDependency);
            }

            for (var index = 1; index < result.Count; index++)
            {
                MoveUp(result, index, LowestIndex(result, index, hasDependency, getPriority));
            }

            return result;
        }

        private static void Add<T>(Node<T> node, ICollection<T> list, IEnumerable<Node<T>> nodes, Func<T, T, bool> hasDependency)
        {
            if (node.Used)
                return;

            node.Used = true;

            foreach (var dependency in nodes.Where(n => hasDependency(node.Item, n.Item)))
            {
                Add(dependency, list, nodes, hasDependency);
            }

            list.Add(node.Item);
        }

        private static int LowestIndex<T>(List<T> list, int index, Func<T, T, bool> hasDependency, Func<T, int> getPriority)
        {
            double priority = getPriority(list[index]);

            var lowestIndex = index;
            for (var i = index - 1; i >= 0; i--)
            {
                if (hasDependency(list[index], list[i]))
                {
                    return lowestIndex;
                }

                double currentPriority = getPriority(list[i]);
                if (currentPriority > priority)
                {
                    lowestIndex = i;
                }
            }

            return lowestIndex;
        }

        private static void MoveUp<T>(List<T> list, int index, int lowerIndex)
        {
            if (index < lowerIndex)
            {
                throw new ArgumentException("Should be higher or equal to 'lowerIndex'.", nameof(index));
            }

            if (index != lowerIndex)
            {
                var temp = list[index];

                for (; index > lowerIndex; index--)
                {
                    list[index] = list[index - 1];
                }

                list[lowerIndex] = temp;
            }
        }
    }
}
