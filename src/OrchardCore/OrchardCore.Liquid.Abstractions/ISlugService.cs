namespace OrchardCore.Liquid
{
    public interface ISlugService
    {
        /// <summary>
        /// Transforms specified text to the form suitable for URL slugs.
        /// </summary>
        /// <param name="text">The text to transform.</param>
        /// <returns>The slug created from the input text.</returns>
        string Slugify(string text);
    }
}
