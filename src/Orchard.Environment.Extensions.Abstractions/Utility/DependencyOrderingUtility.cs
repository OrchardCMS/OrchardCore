using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Utility
{
    public static class DependencyOrdering
    {
        class Linkage<T>
        {
            public T Element { get; set; }
            public bool Used { get; set; }
        }

        /// <summary>
        /// Sort a collection of elements "by dependency order". By passing a lambda which determines if an element
        /// is a dependency of another, this algorithm will return the collection of elements sorted
        /// so that a given element in the sequence doesn't depend on any other element further in the sequence.
        /// </summary>
        public static IEnumerable<T> OrderByDependenciesAndPriorities<T>(this IEnumerable<T> elements, Func<T, T, bool> hasDependency, Func<T, int> getPriority)
        {
            var population = elements.Select(d => new Linkage<T>
            {
                Element = d
            }).OrderBy(item => getPriority(item.Element)).ToArray(); // Performing an initial sorting by priorities may optimize performance

            var result = new List<T>();
            foreach (var item in population)
            {
                Add(item, result, population, hasDependency, getPriority);
            }

            // shift elements forward as possible within priorities and dependencies
            for (int i = 1; i < result.Count; i++)
            {
                int bestPosition = BestPriorityPosition(result, i, hasDependency, getPriority);
                SwitchAndShift(result, i, bestPosition);
            }

            return result;
        }

        private static void Add<T>(Linkage<T> item, ICollection<T> list, IEnumerable<Linkage<T>> population, Func<T, T, bool> hasDependency, Func<T, int> getPriority)
        {
            if (item.Used)
                return;

            item.Used = true;

            foreach (var dependency in population.Where(dep => hasDependency(item.Element, dep.Element)))
            {
                Add(dependency, list, population, hasDependency, getPriority);
            }

            list.Add(item.Element);
        }

        private static int BestPriorityPosition<T>(List<T> list, int index, Func<T, T, bool> hasDependency, Func<T, int> getPriority)
        {
            int bestPriority = getPriority(list[index]);
            int bestIndex = index;

            for (int i = index - 1; i >= 0; i--)
            {
                if (hasDependency(list[index], list[i]))
                {
                    return bestIndex;
                }

                int currentPriority = getPriority(list[i]);
                if (currentPriority > bestPriority)
                {
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// Advances an element within the list from an initial position to a final position with a lower index.
        /// </summary>
        /// <typeparam name="T">The type of each element.</typeparam>
        /// <param name="list">the list of elements.</param>
        /// <param name="initialPosition">The initial position within the list.</param>
        /// <param name="finalPosition">The final position within the list.</param>
        /// <returns>True if any change was made; false otherwise.</returns>
        private static bool SwitchAndShift<T>(List<T> list, int initialPosition, int finalPosition)
        {
            if (initialPosition < finalPosition)
            {
                throw new ArgumentException("finalPosition");
            }

            if (initialPosition != finalPosition)
            {
                T temp = list[initialPosition];

                for (; initialPosition > finalPosition; initialPosition--)
                {
                    list[initialPosition] = list[initialPosition - 1];
                }

                list[finalPosition] = temp;

                return true;
            }

            return false;
        }
    }
}