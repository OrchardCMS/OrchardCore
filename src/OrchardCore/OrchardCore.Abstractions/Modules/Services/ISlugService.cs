namespace OrchardCore.Modules.Services
{
    public interface ISlugService
    {
        /// <summary>
        /// Transforms specified text to the form suitable for URL slugs.
        /// </summary>
        /// <param name="text">The text to transform.</param>
        /// <returns>The slug created from the input text.</returns>
        string Slugify(string text);

        /// <summary>
        /// Transforms specified text to a custom form generally not suitable for URL slugs.
        /// Allows you to use a specified separator char.
        /// </summary>
        /// <param name="text">The text to transform.</param>
        /// <param name="separator">The separator char</param>
        /// <returns>The slug created from the input text.</returns>
        string Slugify(string text, char separator);
    }
}
