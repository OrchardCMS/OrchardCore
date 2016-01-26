using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Title
{
    public interface IPageTitleBuilder : IDependency
    {
        /// <summary>
        /// Clears the current list of segments.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds a segment to the title.
        /// </summary>
        /// <param name="segment">A string to add at the specific location in the title.</param>
        void AddSegment(string segment, string position = "0");

        /// <summary>
        /// Adds some segments to the title.
        /// </summary>
        /// <param name="segments">A set of strings to add at the specific location inthe title.</param>
        void AddSegments(string[] segments, string position = "0");

        /// <summary>
        /// Concatenates every title segments using the separator defined in settings.
        /// </summary>
        /// <returns>A string representing the aggregate title for the current page.</returns>
        string GenerateTitle();
    }
}
