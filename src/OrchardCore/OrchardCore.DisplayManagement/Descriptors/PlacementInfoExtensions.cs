using System;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public static class PlacementInfoExtensions
    {
        /// <summary>
        /// Combines two <see cref="PlacementInfo"/>.
        /// </summary>
        /// <remarks>
        /// Second overrides first. First and second can be null.
        /// </remarks>
        /// <param name="first">First placement.</param>
        /// <param name="second">Second placement.</param>
        /// <returns>Combined <see cref="PlacementInfo"/>.</returns>
        public static PlacementInfo Combine(this PlacementInfo first, PlacementInfo second)
        {
            if (first == null)
            {
                return second;
            }
            else if (second != null)
            {
                var combined = new PlacementInfo();

                combined.Alternates = first.Alternates.Combine(second.Alternates);
                combined.Wrappers = first.Wrappers.Combine(second.Wrappers);

                combined.ShapeType = string.IsNullOrEmpty(second.ShapeType) ? first.ShapeType : second.ShapeType;
                combined.Location = string.IsNullOrEmpty(second.Location) ? first.Location : second.Location;
                combined.DefaultPosition = string.IsNullOrEmpty(second.DefaultPosition) ? first.DefaultPosition : second.DefaultPosition;
                combined.Source = $"{first.Source},{second.Source}";

                return combined;
            }

            return first;
        }

        /// <summary>
        /// Combines two <see cref="AlternatesCollection"/>.
        /// </summary>
        /// <remarks>
        /// First and second can be null.
        /// </remarks>
        /// <param name="first">First collection.</param>
        /// <param name="second">Second collection.</param>
        /// <returns>Combined <see cref="AlternatesCollection"/>.</returns>
        public static AlternatesCollection Combine(this AlternatesCollection first, AlternatesCollection second)
        {
            if (first == null)
            {
                return second;
            }
            else if (second != null)
            {
                var combined = new AlternatesCollection();

                combined.AddRange(first);
                combined.AddRange(second);

                return combined;
            }

            return first;
        }
    }
}
