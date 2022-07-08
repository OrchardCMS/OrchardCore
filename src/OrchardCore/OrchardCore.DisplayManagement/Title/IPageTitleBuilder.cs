using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Title
{
    public interface IPageTitleBuilder
    {
        /// <summary>
        /// Sets a fixed title that will be used instead of any segmented titles added later.
        /// Can be cleared with <see cref="Clear()"/>.
        /// </summary>
        void SetFixedTitle(IHtmlContent title);

        /// <summary>
        /// Clears the current list of segments.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds a segment to the title.
        /// </summary>
        /// <param name="segment">A segments to add at the specific location in the title.</param>
        /// <param name="position">The position, defaults to 0.</param>
        void AddSegment(IHtmlContent segment, string position = "0");

        /// <summary>
        /// Concatenates every title segments using the separator defined in settings.
        /// </summary>
        /// <param name="separator">The html string that should separate all segments.</param>
        /// <returns>A string representing the aggregate title for the current page.</returns>
        IHtmlContent GenerateTitle(IHtmlContent separator);
    }

    public static class PageTitleBuilderExtensions
    {
        /// <summary>
        /// Concatenates every title segments using the separator defined in settings.
        /// </summary>
        /// <returns>A string representing the aggregate title for the current page.</returns>
        public static IHtmlContent GenerateTitle(this IPageTitleBuilder builder)
        {
            return builder.GenerateTitle(null);
        }
    }
}
