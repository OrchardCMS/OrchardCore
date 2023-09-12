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
                first.Alternates = first.Alternates.Combine(second.Alternates);
                first.Wrappers = first.Wrappers.Combine(second.Wrappers);
                if (!String.IsNullOrEmpty(second.ShapeType))
                {
                    first.ShapeType = second.ShapeType;
                }

                if (!String.IsNullOrEmpty(second.Location))
                {
                    first.Location = second.Location;
                }

                if (!String.IsNullOrEmpty(second.DefaultPosition))
                {
                    first.DefaultPosition = second.DefaultPosition;
                }

                if (!String.IsNullOrEmpty(second.Source))
                {
                    first.Source += "," + second.Source;
                }
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
                first.AddRange(second);
            }

            return first;
        }
    }
}
