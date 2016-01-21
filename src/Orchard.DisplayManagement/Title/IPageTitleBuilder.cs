using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Title
{
    public interface IPageTitleBuilder : IDependency
    {

        /// <summary>
        /// Adds a strings to the title.
        /// </summary>
        /// <param name="titleParts">A string to add at the specific location in the title.</param>
        void AddTitlePart(string titlePart, string position = "0");

        /// <summary>
        /// Adds some strings to the title.
        /// </summary>
        /// <param name="titleParts">A set of strings to add at the specific location inthe title.</param>
        void AddTitleParts(string[] titlePart, string position = "0");

        /// <summary>
        /// Concatenates every title parts using the separator defined in settings.
        /// </summary>
        /// <returns>A string representing the aggregate title for the current page.</returns>
        string GenerateTitle();
    }
}
